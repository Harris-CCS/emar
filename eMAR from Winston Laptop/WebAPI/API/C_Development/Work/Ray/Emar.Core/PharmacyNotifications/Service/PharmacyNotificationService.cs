using Emar.Core.Helpers;
using Emar.Core.Orders.Repository;
using Emar.Core.Sites.Repository;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Emar.Core.PharmacyNotifications
{
    public class PharmacyNotificationService : IPharmacyNotificationService, IHostedService, IDisposable
    {
        private const string name = "Pharmacy Notifications Service";

        private readonly ILogger<PharmacyNotificationService> _logger;
        private readonly IServiceProvider _service;

        private Timer _timer;

        // Default cycle interval
        private static int cycleSecondsDefault = 60;

        // Configured intervals
        private static int cycleSeconds = cycleSecondsDefault;
        private static int resendSeconds = 0;

        public PharmacyNotificationService(IServiceProvider service, ILogger<PharmacyNotificationService> logger)
        {
            _service = service;
            _logger = logger;

            try
            {
                IniFile iniFile = new IniFile();
                cycleSeconds = GetSecondsFromConfigInterval(iniFile, "Cycle");
                resendSeconds = GetSecondsFromConfigInterval(iniFile, "Resend_interval");

                _logger.LogInformation("{name} cycle time: {cycleSeconds}. resend interval: {resendSeconds}", name, cycleSeconds, resendSeconds);
            }
            catch (Exception e)
            {
                _logger.LogError("{name} exception while reading INI file for pharmacy notifications", name);
                LogException(e);
            }
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{name} started", name);
            _timer = new Timer(CheckQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(cycleSeconds));

            return Task.CompletedTask;
        }

        private async void CheckQueue(object state)
        {
            _logger.LogInformation("{name} checking queue", name);

            using (var scope = _service.CreateScope())
            {
                var emarContext = scope.ServiceProvider.GetRequiredService<EmarContext>();
                var cache = scope.ServiceProvider.GetRequiredService<EmarMemoryCache>();
                var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var siteRepository = scope.ServiceProvider.GetRequiredService<ISiteRepository>();

                if (emarContext == null)
                {
                    throw new NullReferenceException("Scoped Service EmarContext not available in " + name);
                }
                if (cache == null)
                {
                    throw new NullReferenceException("Scoped Service EmarMemoryCache not available in " + name);
                }
                if (orderRepository == null)
                {
                    throw new NullReferenceException("Scoped Service IOrderRepository not available in " + name);
                }
                if (siteRepository == null)
                {
                    throw new NullReferenceException("Scoped Service ISiteRepository not available in " + name);
                }

                DateTimeOffset resendLimit = DateTimeOffset.Now.AddSeconds(resendSeconds * -1);
                List<PharmacyNotification> notifications = new List<PharmacyNotification>();

                // Grab the notifications we need to process
                try
                {
                    notifications = emarContext.PharmacyNotifications
                        .Include(p => p.Patient)
                            .ThenInclude(s => s.Site)
                        .Where(
                            x => (
                                // Notifications that have not been sent yet
                                (x.CompletedDatetime == null)

                                // Notifications that have been sent before but need to be sent again
                                // (Only if we're performing resends)
                                || (resendSeconds > 0 && x.CompletedDatetime != null && x.CompletedDatetime <= resendLimit)
                            )
                        ).ToList();
                }
                catch (Exception e)
                {
                    _logger.LogError("{name} exception while querying notification queue");
                    LogException(e);
                    return;
                }

                if (notifications == null || notifications.Count == 0)
                {
                    _logger.LogInformation("{name} no records found in queue", name);
                    return;
                }

                // If we have notifications to send, read the ini files for notification format and other details we need
                IniFile iniFile = new IniFile();

                // Order template content and fields
                string orderTemplateContent = GetFileContentFromConfig(iniFile, "Order");
                List<string> orderTemplateFields = ParseFieldsFromTemplate(orderTemplateContent);

                // Administration template content and fields
                string adminTemplateContent = GetFileContentFromConfig(iniFile, "Administration");
                List<string> adminTemplateFields = ParseFieldsFromTemplate(adminTemplateContent);

                // Default mail subject, plus type-specific subjects
                string mailSubject = GetGeneralStringFromConfig(iniFile, "Subject");
                string orderMailSubject = GetGeneralStringFromConfig(iniFile, "Subject_Order");
                string administrationMailSubject = GetGeneralStringFromConfig(iniFile, "Subject_Administration");

                // Other mailing details
                string defaultMailRecipients = GetGeneralStringFromConfig(iniFile, "Recipient");
                string mailSender = GetGeneralStringFromConfig(iniFile, "Sender");
                string serverAddress = GetGeneralStringFromConfig(iniFile, "SMTP_server_address");
                string serverPort = GetGeneralStringFromConfig(iniFile, "SMTP_server_port");
                int? port = int.TryParse(serverPort, out int number) ? number : (int?)null;
                string serverUsername = GetGeneralStringFromConfig(iniFile, "SMTP_server_username");
                string serverPassword = GetGeneralStringFromConfig(iniFile, "SMTP_server_password");
                bool serverSSL = GetGeneralStringFromConfig(iniFile, "SMTP_server_use_ssl") == "Y" ? true : false;

                if (string.IsNullOrWhiteSpace(serverAddress) || port == null)
                {
                    _logger.LogError("{name} missing required SMTP configuration information. Cannot send notifications.", name);
                    return;
                }

                SmtpClient smtpClient = null;
                try
                {
                    if (IsBase64String(serverPassword))
                    {
                        serverPassword = Encoding.UTF8.GetString(Convert.FromBase64String(serverPassword));
                    }

                    smtpClient = new SmtpClient(serverAddress)
                    {
                        Port = (int)port,
                        EnableSsl = serverSSL,
                    };

                    if (!string.IsNullOrWhiteSpace(serverUsername) && !string.IsNullOrWhiteSpace(serverPassword))
                    {
                        smtpClient.Credentials = new NetworkCredential(serverUsername, serverPassword);
                    }
                    else
                    {
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("{name} exception while setting up SMTP client. Cannot send notifications.", name);
                    LogException(e);
                    return;
                }

                List<MailMessage> notificationMails = new List<MailMessage>();
                Dictionary<int, string> mailRecipients = new Dictionary<int, string>();

                foreach (PharmacyNotification notification in notifications)
                {
                    Patient patient = notification.Patient;
                    Site site = patient.Site;

                    string timeZoneName = siteRepository.GetSiteTimeZone(site.Id);
                    string notificationTemplateContent = "";
                    string notificationSubject = mailSubject;
                    List<string> notificationTemplateFields = new List<string>();
                    List<PatientOrder> orders = new List<PatientOrder>();
                    bool isComboMed = false;

                    // Admin notification type - grab the admin(s) associated with this notification ID, then grab their associated orders and put them together.
                    if (notification.Type == "Administration")
                    {
                        if (administrationMailSubject != null && administrationMailSubject.Length > 0)
                        {
                            notificationSubject = administrationMailSubject;
                        }
                        notificationTemplateContent = adminTemplateContent;
                        notificationTemplateFields = adminTemplateFields;
                        List<PharmacyNotificationAdministration> pharmacyNotifications =
                            emarContext.PharmacyNotificationAdministrations.Where(x => x.PharmacyNotificationId == notification.Id).ToList();

                        if (pharmacyNotifications != null && pharmacyNotifications.Count > 0)
                        {
                            List<OrderAdministration> orderAdministrations =
                                emarContext.OrderAdministrations.Where(
                                    x => pharmacyNotifications
                                            .Select(o => o.OrderAdministrationId)
                                            .Contains(x.Id)
                                ).ToList();

                            List<long> orderAdminIds = orderAdministrations.Select(x => x.Id).ToList();
                            if (orderAdminIds != null && orderAdminIds.Count > 0)
                            {
                                orders = emarContext.PatientOrders.Where(
                                    o => o.OrderAdministrations
                                            .Any(a => orderAdminIds.Contains(a.Id))
                                    )
                                    .Include(f => f.FrequencySchedule)
                                    .ToList();

                                foreach (PatientOrder order in orders)
                                {
                                    OrderAdministration matchingAdmin = orderAdministrations.Where(x => x.PatientOrderId == order.Id).FirstOrDefault();
                                    if (matchingAdmin != null)
                                    {
                                        order.OrderAdministrations.Add(matchingAdmin);
                                    }
                                }
                            }
                        }
                    }

                    // Order notification type - grab additional objects we need
                    else
                    {
                        if (orderMailSubject != null && orderMailSubject.Length > 0)
                        {
                            notificationSubject = orderMailSubject;
                        }
                        notificationTemplateContent = orderTemplateContent;
                        notificationTemplateFields = orderTemplateFields;
                        List<PharmacyNotificationOrder> pharmacyNotifications =
                            emarContext.PharmacyNotificationOrders.Where(x => x.PharmacyNotificationId == notification.Id).ToList();

                        if (pharmacyNotifications != null && pharmacyNotifications.Count > 0)
                        {
                            orders =
                                emarContext.PatientOrders.Where(
                                    x => pharmacyNotifications
                                            .Select(p => p.PatientOrderId)
                                            .Contains(x.Id)
                                )
                                .Include(f => f.FrequencySchedule)
                                .ToList();
                        }
                    }

                    // In the case of multiple orders in one notification, modify the template content by finding the grouped 
                    // order/admin/med-specific lines and duplicating them as many times as necessary to have a field for each order.
                    if (orders.Count > 1)
                    {
                        int? firstLinePosition = null;
                        int? lastLinePosition = null;
                        int lineNum = 0;
                        List<string> lines = notificationTemplateContent.Split("\n").ToList();
                        foreach (string line in lines)
                        {
                            List<string> lineFields = ParseFieldsFromTemplate(line);
                            foreach (string field in lineFields)
                            {
                                string[] mapResult = field.Split(".");
                                string fieldType = mapResult[0];
                                if (fieldType == "Order" || fieldType == "OrderAdministration" || fieldType.StartsWith("Med"))
                                {
                                    if (firstLinePosition == null)
                                    {
                                        firstLinePosition = lineNum;
                                    }
                                    else
                                    {
                                        lastLinePosition = lineNum;
                                    }

                                    break;
                                }
                            }
                            lineNum++;
                        }

                        if (firstLinePosition != null)
                        {
                            if (lastLinePosition == null)
                            {
                                lastLinePosition = firstLinePosition;
                            }

                            List<string> duplicateLines = lines.GetRange((int)firstLinePosition, (int)lastLinePosition + 1 - (int)firstLinePosition);
                            duplicateLines.Add("");
                            if (lastLinePosition == 0)
                            {
                                lines.Insert((int)lastLinePosition + 1, "");
                            }

                            for (int i = 1; i < orders.Count; i++)
                            {
                                lines.InsertRange((int)firstLinePosition, duplicateLines);
                            }

                            notificationTemplateContent = String.Join("\n", lines).Trim();
                        }
                    }

                    //  Skip the rest of the processing if we don't have a notification template to use.
                    if (string.IsNullOrWhiteSpace(notificationTemplateContent))
                    {
                        continue;
                    }

                    // Replace Site- or Patient-specific fields in the subject of the message
                    List<string> subjectFields = ParseFieldsFromTemplate(notificationSubject);
                    foreach (string field in subjectFields)
                    {
                        string templateSearch = "[" + field + "]";
                        string templateValue = templateSearch;
                        string[] mapResult = field.Split(".");
                        string fieldType = mapResult[0];
                        string fieldValue = mapResult[1];
                        Object dbObject = null;

                        switch (fieldType)
                        {
                            case "Patient":
                                dbObject = patient;
                                break;
                            case "Site":
                                dbObject = site;
                                break;
                        }

                        if (dbObject != null)
                        {
                            templateValue = FormatPropertyValue(dbObject, fieldValue, timeZoneName);
                        }

                        notificationSubject = notificationSubject.Replace(templateSearch, templateValue, StringComparison.InvariantCultureIgnoreCase);
                    }

                    foreach (PatientOrder order in orders)
                    {
                        if (order == null)
                        {
                            continue;
                        }

                        Medication medication = null;
                        MedicationDetail medicationDetail = null;
                        MedicationUnit medicationUnit = null;
                        MedicationRoute medicationRoute = null;
                        FrequencySchedule frequencySchedule = order.FrequencySchedule;
                        OrderAdministration orderAdministration = order.OrderAdministrations != null && order.OrderAdministrations.Count > 0 ? order.OrderAdministrations.FirstOrDefault() : null;
                        DurationUnit duration = null;

                        try
                        {
                            // Get duration details
                            if (order.DurationUnitId != null)
                            {
                                duration = orderRepository.GetDurationUnits().FirstOrDefault(x => x.Id == order.DurationUnitId);
                            }

                            // Get the medication and medication details associated with the order
                            medication = emarContext.Medications.FirstOrDefault(x => x.Id == order.MedicationId);
                            if (medication != null)
                            {
                                int? medUnitId = order.MedicationUnitId;
                                int? medRouteId = order.MedicationRouteId;
                                isComboMed = (medication.DrugId != null && medication.DrugId.Equals("COMBO"));
                                if (isComboMed)
                                {
                                    medicationDetail = emarContext.MedicationDetails.FirstOrDefault(x => x.MedicationId == medication.Id);
                                    medUnitId = medicationDetail.MedicationUnitId;
                                }

                                if (medUnitId != null)
                                {
                                    int? medUnitSite = GetMedUnitSite(site.Id, cache, emarContext);
                                    if (medUnitSite != null)
                                    {
                                        medicationUnit = orderRepository.GetUnits((int)medUnitSite).FirstOrDefault(x => x.Id == medUnitId);
                                    }
                                }

                                if (medRouteId != null)
                                {
                                    int? medRouteSite = GetMedRouteSite(site.Id, cache, emarContext);
                                    if (medRouteSite != null)
                                    {
                                        medicationRoute = orderRepository.GetRoutes((int)medRouteSite).FirstOrDefault(x => x.Id == medRouteId);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("{name} exception while querying for associated objects");
                            LogException(e);
                            return;
                        }

                        try
                        {
                            // Loop over the fields we found in the notification template and replace them with
                            // the values from their associated objects (if they are mapped), which results in
                            // empty values in the template for null or empty object values.
                            // If they are not mapped, don't change them.
                            foreach (string field in notificationTemplateFields)
                            {
                                string templateSearch = "[" + field + "]";
                                string templateValue = templateSearch;
                                bool isDuplicateReplacement = true;

                                string[] mapResult = field.Split(".");
                                string fieldType = mapResult[0];

                                // Switch object used for dose if necessary. Combo med takes it from medication_details.
                                if (isComboMed && field == "Order.Dose")
                                {
                                    fieldType = "MedDetail";
                                }

                                string fieldValue = mapResult[1];
                                Object dbObject = null;
                                switch (fieldType)
                                {
                                    case "Patient":
                                        dbObject = patient;
                                        isDuplicateReplacement = false;
                                        break;
                                    case "Site":
                                        dbObject = site;
                                        isDuplicateReplacement = false;
                                        break;
                                    case "Medication":
                                        dbObject = medication;
                                        break;
                                    case "MedDetail":
                                        dbObject = medicationDetail;
                                        break;
                                    case "MedFrequency":
                                        dbObject = frequencySchedule;
                                        break;
                                    case "MedRoute":
                                        dbObject = medicationRoute;
                                        break;
                                    case "MedUnit":
                                        dbObject = medicationUnit;
                                        break;
                                    case "Order":
                                        dbObject = order;
                                        break;
                                    case "OrderDuration":
                                        dbObject = duration;
                                        break;
                                    case "OrderAdministration":
                                        dbObject = orderAdministration;
                                        break;
                                }

                                if (dbObject != null)
                                {
                                    templateValue = FormatPropertyValue(dbObject, fieldValue, timeZoneName);
                                }

                                // In the case of fields that are potentially duplicated in the template because of multiple orders, we need to only replace
                                // the first instance of the field that we find, because subsequent orders will take the other instances.
                                if (orders.Count > 1 && isDuplicateReplacement)
                                {
                                    notificationTemplateContent = new Regex(Regex.Escape(templateSearch), RegexOptions.IgnoreCase).Replace(notificationTemplateContent, templateValue, 1);
                                }
                                // Non-duplicated fields can replace all instances. Though I'm not sure there would ever be multiple...
                                else
                                {
                                    notificationTemplateContent = notificationTemplateContent.Replace(templateSearch, templateValue, StringComparison.InvariantCultureIgnoreCase);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("{name} exception while generating pharmacy notification content", name);
                            LogException(e);
                        }
                    }

                    notification.CompletedDatetime = DateTimeOffset.Now;

                    // Build the notification mail message after we've modified the template with our new content
                    try
                    {
                        bool useHTML = Regex.IsMatch(notificationTemplateContent, @"<\/?\w+\s*\/?\s*>");
                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(mailSender),
                            Sender = new MailAddress(mailSender),
                            Subject = notificationSubject,
                            Body = notificationTemplateContent,
                            IsBodyHtml = useHTML
                        };

                        if (!mailRecipients.ContainsKey(site.Id))
                        {
                            mailRecipients.Add(site.Id, GetGeneralStringFromConfig(iniFile, "Recipient_" + site.Id));
                        }

                        string useRecipients =
                            mailRecipients[site.Id].Length > 0 ? mailRecipients[site.Id] : defaultMailRecipients;

                        if (useRecipients.Length > 0)
                        {
                            foreach (string recipient in useRecipients.Split(","))
                            {
                                mailMessage.To.Add(recipient.Trim());
                            }

                            notificationMails.Add(mailMessage);
                        }
                        else
                        {
                            _logger.LogError("{name} did not find recipients configured for site {site}. Cannot send notifications.", name, site.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("{name} exception while generating pharmacy notification email", name);
                        LogException(e);
                    }
                }

                // Save CompletedDatetime changes
                emarContext.SaveChanges();

                // Send notification emails
                foreach (MailMessage message in notificationMails)
                {
                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("{name} exception while sending pharmacy notification email", name);
                        LogException(e);
                    }
                }

                smtpClient.Dispose();
            }

            _logger.LogInformation("{name} queue processing complete", name);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{name} stopping", name);
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private string FormatPropertyValue(Object dbObject, string dbValueName, string timeZoneName)
        {
            if (dbObject == null || String.IsNullOrWhiteSpace(dbValueName))
            {
                return "";
            }

            PropertyInfo? propertyInfo = dbObject.GetType().GetProperty(dbValueName);
            if (propertyInfo == null)
            {
                return "";
            }

            Object result = propertyInfo.GetValue(dbObject, null);
            if (result == null)
            {
                return "";
            }

            if (result is bool || result is Boolean)
            {
                return (bool)result ? "Yes" : "No";
            }
            else if (result is DateTime)
            {
                DateTime df = (DateTime)result;

                // If the time part of this datetime is exactly 00:00:00.00, assume we only store a date and therefore only display the date.
                if (df.Hour == 0 && df.Minute == 0 && df.Second == 0 && df.Millisecond == 0)
                {
                    return df.ToShortDateString();
                }

                return df.ToShortDateString() + " " + df.ToLongTimeString();
            }
            else if (result is DateTimeOffset)
            {
                DateTimeOffset dto = DateTimeOffsetExtensions.TimeAdjustedForTimeZone(timeZoneName, (DateTimeOffset)result);
                return dto.DateTime.ToShortDateString() + " " + dto.DateTime.ToLongTimeString();
            }
            else if (result is Decimal)
            {
                // Magic to remove trailing zeroes
                result = ((Decimal)result / 1.000000000000000000000000000000000m).ToString();
            }

            return result.ToString();
        }

        private string GetFileContentFromConfig(IniFile iniFile, string fileKey)
        {
            string fileContent = "";
            var value = iniFile.Read(fileKey, "Files");
            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    fileContent = File.ReadAllText(value);
                }
                catch (Exception e)
                {
                    _logger.LogError("{name} exception while reading template file {filekey} for pharmacy notifications", name, fileKey);
                    LogException(e);
                }
            }

            return fileContent;
        }

        private string GetGeneralStringFromConfig(IniFile iniFile, string configKey)
        {
            var value = iniFile.Read(configKey, "General");
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

        private int? GetMedRouteSite(int siteId, EmarMemoryCache cache, EmarContext emarContext)
        {
            return cache.Cache.GetOrCreate(siteId + CacheKeys.RouteSites, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                var ret = emarContext.SiteCodeShares.FirstOrDefault(s => s.SourceSiteId == siteId && s.Entity == "medication_routes")?.TargetSiteId;
                entry.Size = 1;
                return ret;
            });
        }

        private int? GetMedUnitSite(int siteId, EmarMemoryCache cache, EmarContext emarContext)
        {
            return cache.Cache.GetOrCreate(siteId + CacheKeys.UnitSites, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                var ret = emarContext.SiteCodeShares.FirstOrDefault(s => s.SourceSiteId == siteId && s.Entity == "medication_units")?.TargetSiteId;
                entry.Size = 1;
                return ret;
            });
        }

        private int GetSecondsFromConfigInterval(IniFile iniFile, string intervalKey)
        {
            var value = GetGeneralStringFromConfig(iniFile, intervalKey);
            int result = 0;
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.ToString().ToUpperInvariant();
                Match match = Regex.Match(value, @"^([.0-9]+)([DHMS])$");
                if (match.Success)
                {
                    double timeValue = Double.Parse(match.Groups[1].Value);
                    string timeUnit = match.Groups[2].Value;

                    switch (timeUnit)
                    {
                        case "D":
                            timeValue *= (60 * 60 * 24);
                            break;
                        case "H":
                            timeValue *= (60 * 60);
                            break;
                        case "M":
                            timeValue *= 60;
                            break;
                        default:
                            break;
                    }

                    if (timeValue > 0)
                    {
                        result = (int)timeValue;
                    }
                }
            }

            if (result == 0 && intervalKey == "Cycle") {
                result = cycleSecondsDefault;
            }

            return result;
        }
        private bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }

        private void LogException(Exception ex)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                string sException = ex.Message + "\n";
                if (ex is SqlException)
                {
                    sException += "error number = " + ((SqlException)ex).Number + "\n";
                }
                sException += "source = " + ex.Source + "\n";
                if (ex is SqlException)
                {
                    sException += "Line Number = " + ((SqlException)ex).LineNumber + "\n";
                }
                sException += ex.StackTrace + "\n";

                if (!(ex is SqlException))
                {
                    sException += ex.InnerException + "\n"; // added inner exception
                }

                eventLog.Source = "PulseCheck EMAR API";
                eventLog.WriteEntry(sException, EventLogEntryType.Information, 101, 1);
            }
        }

        private List<string> ParseFieldsFromTemplate(string templateContent)
        {
            List<string> fields = new List<string>();

            MatchCollection matches = Regex.Matches(templateContent, @"\[(\w+\.\w+)\]", RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                fields.Add(match.Groups[1].Value);
            }

            return fields.Distinct().ToList();
        }
    }
}

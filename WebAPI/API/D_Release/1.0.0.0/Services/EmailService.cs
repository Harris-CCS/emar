using DomainModel.Membership;
using Interfaces.Services;
using PulseCheck.Utilities;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using System;

namespace Services
{
    public class EmailService : IEmailService
    {
        public async Task<bool> SendNewAccountEmail(UserAccount account, string accessToken, string devicePasscode)
        {
            var passwordUrl = GetPasswordChangeUrl(accessToken);
            var apiUrl = GetApiUrl();
            var emailModel = new DomainModel.MobileEmail
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                LinkUrl = passwordUrl,
                APIUrl = apiUrl,
                DevicePasscode = devicePasscode,
            };

            var body = Engine.Razor.Run(DomainModel.Constants.EmailTemplates.NEW_ACCOUNT, typeof(DomainModel.MobileEmail), emailModel);

            var sentMail = await SendEmail(account.Email, "New PulseCheck Master Account", body);

            return sentMail;
        }

        public async Task<bool> SendAccountPasswordResetEmail(UserAccount account, string accessToken)
        {
            var pcUrl = GetPasswordChangeUrl(accessToken);
            var emailModel = new DomainModel.MobileEmail
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                LinkUrl = pcUrl
            };

            var body = Engine.Razor.Run(DomainModel.Constants.EmailTemplates.PASSWORD_RESET, typeof(DomainModel.MobileEmail), emailModel);

            var sentMail = await SendEmail(account.Email, "PulseCheck Password Reset", body);

            return sentMail;
        }

        public async Task<bool> SendDeviceAuthorizationEmail(UserAccount account, string devicePasscode)
        {
            var apiUrl = GetApiUrl();
            var emailModel = new DomainModel.MobileEmail
            {
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                LinkUrl = apiUrl,
                DevicePasscode = devicePasscode,
                APIUrl = apiUrl,
            };

            var body = Engine.Razor.Run(DomainModel.Constants.EmailTemplates.DEVICE_AUTHORIZATION, typeof(DomainModel.MobileEmail), emailModel);

            var sentMail = await SendEmail(account.Email, "PulseCheck Device Authorization", body);

            return sentMail;
        }

        private string GetApiUrl()
        {
            var currentVersion = PulseCheck.Utilities.Version.APIVersion;
            var apiUrl = new DB.Select
            {
                Sql = "SELECT api_base_address FROM api_settings where active=1 and api_major_version=@major and api_minor_version=@minor",
                Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@major", SqlDbType.Int) { Value = currentVersion.MajorVersion },
                        new SqlParameter("@minor", SqlDbType.Int) { Value = currentVersion.MinorVersion },
                    }
            }.RunForScalar().ToString();

            return apiUrl;
        }

        private string GetPasswordChangeUrl(string accessToken)
        {
            var pcURL = ConfigurationManager.AppSettings["PulseCheckURL"].Trim();
            if (!pcURL.EndsWith("/"))
                pcURL += "/";

            return pcURL + "account_password.ibx?token=" + accessToken;
        }

        private async Task<bool> SendEmail(string recipient, string subject, string body)
        {
            SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            var client = new SmtpClient
            {
                Port = section.Network.Port,
                Host = section.Network.Host,
                Credentials = new NetworkCredential(section.Network.UserName, section.Network.Password),
                EnableSsl = section.Network.EnableSsl,
            };

            var mail = new MailMessage(section.From, recipient)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            var sentMail = false;
            try
            {
                await client.SendMailAsync(mail);
                sentMail = true;
            }
            catch (Exception ex)
            {
                sentMail = false;
                var nonFatal = new Exceptions.NonFatalException(ex.Message, ex.InnerException);
                throw nonFatal;
            }
            finally
            {
                client.Dispose();
            }

            return sentMail;
        }
    }
}

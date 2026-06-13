using Emar.Core.Patients.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Notifications.Model.Mappings
{
    /// <summary>
    /// Notification Mapper
    /// </summary>
    public static class NotificationMapper
    {
        /// <summary>
        /// Map a Notification to a NotificationDto
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public static NotificationDto MapNotification(Notification notification, int userId)
        {
            if (notification == null)
            {
                return null;
            }

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                EventTime = notification.EventDateTime,
                NotificationTime = notification.GeneratedDateTime,
                Category = notification.Category == null ? null : notification.Category.Description,
                Title = notification.Title,
                Body = notification.Body,
                ActionUrl = notification.Category == null ? null : notification.Category.ActionUrl,
                Patient = (notification.PatientOrder != null && notification.PatientOrder.Patient != null) ? 
                    PatientMapper.MapPatient(notification.PatientOrder.Patient, userId) : 
                    null
            };

            // ActionURL translation.
            // {PATIENT.ID}, {SITE.ID}, and {USER.ID} (case-insensitive) will be removed from the URL and replaced
            // with their values from the notification (removed entirely if no replacement value is available)
            if (notificationDto.ActionUrl != null && notificationDto.ActionUrl.Length > 0)
            {
                string actionUrl = notificationDto.ActionUrl;

                string patientIdReplacement = "", siteIdReplacement = "";
                if (notificationDto.Patient != null)
                {
                    patientIdReplacement = notificationDto.Patient.Id.ToString();
                    siteIdReplacement = notificationDto.Patient.SiteId.ToString();
                }

                actionUrl = actionUrl
                    .Replace("{PATIENT.ID}", patientIdReplacement, System.StringComparison.InvariantCultureIgnoreCase)
                    .Replace("{SITE.ID}", siteIdReplacement, System.StringComparison.InvariantCultureIgnoreCase)
                    .Replace("{USER.ID}", userId.ToString(), System.StringComparison.InvariantCultureIgnoreCase);

                notificationDto.ActionUrl = actionUrl;
            }

            return notificationDto;
        }
    }
}
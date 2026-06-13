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

            return notificationDto;
        }
    }
}
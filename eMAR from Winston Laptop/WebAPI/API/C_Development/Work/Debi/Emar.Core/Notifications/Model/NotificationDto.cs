using Emar.Core.Patients.Model;
using System;

namespace Emar.Core.Notifications.Model
{
    public class NotificationDto
    {
        /// <summary>
        /// Unique Notification identifier
        /// </summary>
        public long Id { get; set; }

        public DateTimeOffset NotificationTime { get; set; }

        public DateTimeOffset? EventTime { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string Category { get; set; }

        public string ActionUrl { get; set; }

        public PatientDto? Patient { get; set; }
    }
}

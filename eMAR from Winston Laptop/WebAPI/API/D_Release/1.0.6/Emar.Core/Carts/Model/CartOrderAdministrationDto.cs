using System;

namespace Emar.Core.Carts.Model
{
    public class CartOrderAdministrationDto
    {
        /// <summary>
        /// Unique cart order administration identifier.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique cart order identifier.
        /// </summary>
        public long PatientCartOrderId { get; set; }

        /// <summary>
        /// Date and time the order administration is scheduled to start.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset AdministrationScheduledDatetime { get; set; }

        /// <summary>
        /// Date and time the order administration is scheduled to end.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopScheduledDatetime { get; set; }

        // <summary>
        // Indicates whether the order administration is point-in-time.
        // </summary>
        public bool PointInTime { get; set; }
    }
}

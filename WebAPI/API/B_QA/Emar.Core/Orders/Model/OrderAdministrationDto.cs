using System;
using System.Collections.Generic;

namespace Emar.Core.Orders.Model
{
    public class OrderAdministrationDto
    {
        /// <summary>
        /// Unique order administration identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Date and time the order administration is scheduled to start.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset AdministrationScheduledDatetime { get; set; }

        /// <summary>
        /// Date and time the order administration actually started.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AdministrationInputDatetime { get; set; }

        /// <summary>
        /// Date and time the order administration start was recorder.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AdministrationDatetime { get; set; }

        /// <summary>
        /// Unique user identifier of the user that started the order administration.
        /// </summary>
        public int? AdministeringUserId { get; set; }

        /// <summary>
        /// Date and time the order administration is scheduled to end.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopScheduledDatetime { get; set; }

        /// <summary>
        /// Date and time the order administration actually ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopInputDatetime { get; set; }

        /// <summary>
        /// Date and time the order administration end was recorder.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopDatetime { get; set; }

        /// <summary>
        /// Unique user identifier of the user that ended the order administration.
        /// </summary>
        public int? StopUserId { get; set; }

        /// <summary>
        /// Unique user identifier of the user that acknowledged the order administration.
        /// </summary>
        public int? AcknowledgeUserId { get; set; }

        /// <summary>
        /// Date and time the order administration was acknowledged.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AcknowledgeDatetime { get; set; }

        // <summary>
        // Indicates whether the order administration is point-in-time.
        // </summary>
        public bool PointInTime { get; set; }

        /// <summary>
        /// Indicates whether the order administration is on hold.
        /// </summary>
        public bool OnHold { get; set; }

        /// <summary>
        /// Indicates whether the order administration was missed/skipped.
        /// </summary>
        public bool MissedDose { get; set; }

        /// <summary>
        /// Patient order administration events.
        /// </summary>
        public IEnumerable<OrderEventDto> AdministrationEvents { get; set; }
    }
}

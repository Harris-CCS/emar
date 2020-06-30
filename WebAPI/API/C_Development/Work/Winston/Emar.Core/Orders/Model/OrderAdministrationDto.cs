using System;
using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    public class OrderAdministrationDto : HateOasLinkDto
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
        public DateTimeOffset ScheduledAdministrationTime { get; set; }

        /// <summary>
        /// Date and time the order administration actually started.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? ActualAdministrationTime { get; set; }

        /// <summary>
        /// Date and time the order administration start was recorder.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? SystemAdministrationTime { get; set; }

        /// <summary>
        /// Unique user identifier of the user that started the order administration.
        /// </summary>
        public int? AdministrationUserId { get; set; }

        /// <summary>
        /// Date and time the order administration is scheduled to end.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? ScheduledStopTime { get; set; }

        /// <summary>
        /// Date and time the order administration actually ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? ActualStopTime { get; set; }

        /// <summary>
        /// Date and time the order administration end was recorder.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? SystemStopTime { get; set; }

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
        public DateTimeOffset? AcknowledgeTime { get; set; }

        ///////// <summary>
        ///////// Indicates whether the order administration is continuous.
        ///////// </summary>
        //////public bool Continuous { get; set; }

        /// <summary>
        /// Indicates whether the order administration is on hold.
        /// </summary>
        public bool OnHold { get; set; }

        /// <summary>
        /// Indicates whether the order administration was missed/skipped.
        /// </summary>
        public bool MissedDose { get; set; }

        /// <summary>
        /// Order administration events.
        /// </summary>
        public IEnumerable<OrderEvent>? AdministrationEvents { get; set; }
    }
}

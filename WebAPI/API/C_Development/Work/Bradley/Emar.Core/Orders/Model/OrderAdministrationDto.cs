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

        internal string DateFormat { get; set; } = "dd/MM/yyyy";
        internal string TimeFormat { get; set; } = "HH:mm:ss";

        /// <summary>
        /// Date and time the order administration is scheduled to start.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset AdministrationScheduledDatetime { get; set; }
        public string AdministrationScheduledDate => AdministrationScheduledDatetime.ToString(DateFormat);
        public string AdministrationScheduledTime => AdministrationScheduledDatetime.ToString(TimeFormat);

        /// <summary>
        /// Date and time the order administration actually started.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AdministrationInputDatetime { get; set; }
        public string AdministrationInputDate => AdministrationInputDatetime?.ToString(DateFormat);
        public string AdministrationInputTime => AdministrationInputDatetime?.ToString(TimeFormat);

        /// <summary>
        /// Date and time the order administration start was recorded.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AdministrationDatetime { get; set; }
        public string AdministrationDate => AdministrationDatetime?.ToString(DateFormat);
        public string AdministrationTime => AdministrationDatetime?.ToString(TimeFormat);


        /// <summary>
        /// Unique user identifier of the user that started the order administration.
        /// </summary>
        public int? AdministeringUserId { get; set; }

        /// <summary>
        /// Date and time the order administration is scheduled to end.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopScheduledDatetime { get; set; }
        public string StopScheduledDate => StopScheduledDatetime?.ToString(DateFormat);
        public string StopScheduledTime => StopScheduledDatetime?.ToString(TimeFormat);

        /// <summary>
        /// Date and time the order administration actually ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopInputDatetime { get; set; }
        public string StopInputDate => StopInputDatetime?.ToString(DateFormat);
        public string StopInputTime => StopInputDatetime?.ToString(TimeFormat);

        /// <summary>
        /// Date and time the order administration end was recorder.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? StopDatetime { get; set; }
        public string StopDate => StopDatetime?.ToString(DateFormat);
        public string StopTime => StopDatetime?.ToString(TimeFormat);

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
        public string AcknowledgeDate => AcknowledgeDatetime?.ToString(DateFormat);
        public string AcknowledgeTime => AcknowledgeDatetime?.ToString(TimeFormat);

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

        public string AdministrationStatus
        {
            get
            {
                if (OnHold)
                    return "OnHold";
                if (MissedDose)
                    return "Missed";
                if (AdministrationInputDatetime == null)
                {
                    if (AdministrationScheduledDatetime > DateTimeOffset.Now)
                        return "Pending";
                    return "Late";
                }
                if (PointInTime)
                    return "Given";
                return StopInputDatetime == null ? "OnGoing" : "Given";
            }
        }

        /// <summary>
        /// Patient order administration events.
        /// </summary>
        public IEnumerable<OrderEventDto> AdministrationEvents { get; set; }
    }
}

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
        private string TimeFormat { get; set; } = "HH:mm";

        /// <summary>
        /// Date and time the order administration is scheduled to start.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset AdministrationScheduledDatetime { get; set; }
        public string AdministrationScheduledDate => AdministrationScheduledDatetime.ToString(DateFormat);
        public string AdministrationScheduledTime => AdministrationScheduledDatetime.ToString(TimeFormat);

        /// <summary>
        /// Date and time the order administration actually started.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AdministrationSystemDatetime { get; set; }
        public string AdministrationSystemDate => AdministrationSystemDatetime?.ToString(DateFormat);
        public string AdministrationSystemTime => AdministrationSystemDatetime?.ToString(TimeFormat);

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
        internal bool OnHold { get; set; }

        /// <summary>
        /// Indicates whether the order administration was missed/skipped.
        /// </summary>
        internal bool MissedDose { get; set; }

        /**** CHECK CONSTRAINTs on the OrderAdministration in the DB ***
         * •	IF [point_in_time] = 1
         *      o	[stop_scheduled_datetime], [stop_input_datetime] and [stop_datetime] must all be NULL.
         *      o	can only have one of the following (if 2 or more of them, then CHECK constraint fails)
         *          	[missed_dose] = 1
         *          	[on_hold ] = 1
         *          	[administration_input_datetime] NOT NULL
         * •	IF [point_in_time] = 0,
         *      o	[on_hold] must = 0.
         *      o	[missed_dose] must = 0
         *      o	IF [stop_input_datetime] is not NULL, [administration_input_datetime] must not be NULL
         */

        public enum AdministrationStatuses
        {
            OnHold,
            Missed,
            Pending,
            Late,
            Given,
            OnGoing
        }

        internal AdministrationStatuses AdministrationStatusCode
        {
            get
            {
                if (OnHold)
                    return AdministrationStatuses.OnHold;
                if (MissedDose)
                    return AdministrationStatuses.Missed;
                if (AdministrationSystemDatetime == null)
                {
                    if (AdministrationScheduledDatetime > DateTimeOffset.Now)
                        return AdministrationStatuses.Pending;
                    return AdministrationStatuses.Late;
                }
                if (PointInTime)
                    return AdministrationStatuses.Given;
                return StopInputDatetime == null ? AdministrationStatuses.OnGoing : AdministrationStatuses.Given;
            }
        }

        public string AdministrationStatus => AdministrationStatusCode.ToString();

        public DateTimeOffset? TimeNeedingAction 
        {
            get
            {
                switch (AdministrationStatusCode)
                {
                    case AdministrationStatuses.Given:
                    case AdministrationStatuses.Missed:
                        return null;
                    case AdministrationStatuses.Late:
                    case AdministrationStatuses.Pending:
                    case AdministrationStatuses.OnHold:
                        return AdministrationScheduledDatetime;
                    case AdministrationStatuses.OnGoing:
                        return StopScheduledDatetime;
                    default:
                        throw new ArgumentOutOfRangeException(null, AdministrationStatusCode, "From OrderAdministrationDto.TimeNeedingAction Property.");
                }
            }
        }

        /// <summary>
        /// Patient order administration events.
        /// </summary>
        public IEnumerable<OrderEventDto> AdministrationEvents { get; set; }

        public IEnumerable<AvailableActionDto> AvailableActions { get; set; }
    }

}

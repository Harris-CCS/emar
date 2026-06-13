using System;
using System.Collections.Generic;
using Emar.Core.Templates.Model;
using Emar.Core.Users.Model;

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
        public DateTimeOffset? AdministrationSystemDatetime { get; set; }

        /// <summary>
        /// Date and time the order administration start was recorded.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? AdministrationDatetime { get; set; }


        /// <summary>
        /// Unique user identifier of the user that started the order administration.
        /// </summary>
        internal int? AdministeringUserId { get; set; }

        public UserDto AdministeringUser { get; set; }

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
        internal int? StopUserId { get; set; }

        public UserDto StopUser { get; set; }

        /// <summary>
        /// Unique user identifier of the user that acknowledged the order administration.
        /// </summary>
        internal int? AcknowledgeUserId { get; set; }

        public UserDto AcknowledgeUser { get; set; }


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

        internal AdministrationStatusEnum AdministrationStatusCode
        {
            get
            {
                if (OnHold)
                    return AdministrationStatusEnum.OnHold;
                if (MissedDose)
                    return AdministrationStatusEnum.Missed;
                
                if (AdministrationSystemDatetime == null)
                {
                    return AdministrationScheduledDatetime > DateTimeOffset.Now 
                        ? AdministrationStatusEnum.Pending 
                        : AdministrationStatusEnum.Late;
                }

                if (PointInTime)
                    return AdministrationStatusEnum.Given;
                
                return StopInputDatetime == null 
                    ? AdministrationStatusEnum.OnGoing 
                    : AdministrationStatusEnum.Given;
            }
        }

        public string AdministrationStatus => AdministrationStatusCode.ToString();

        public DateTimeOffset? TimeNeedingAction 
        {
            get
            {
                return AdministrationStatusCode switch
                {
                    AdministrationStatusEnum.Given => null,
                    AdministrationStatusEnum.Missed => null,
                    AdministrationStatusEnum.Late => AdministrationScheduledDatetime,
                    AdministrationStatusEnum.Pending => AdministrationScheduledDatetime,
                    AdministrationStatusEnum.OnHold => AdministrationScheduledDatetime,
                    AdministrationStatusEnum.OnGoing => StopScheduledDatetime,
                    _ => throw new ArgumentOutOfRangeException(null, AdministrationStatusCode,
                        "From OrderAdministrationDto.TimeNeedingAction Property.")
                };
            }
        }

        /// <summary>
        /// Patient order administration events.
        /// </summary>
        public IEnumerable<OrderEventDto> AdministrationEvents { get; set; }

        public IEnumerable<AvailableActionDto> AvailableActions { get; set; }
    }

}

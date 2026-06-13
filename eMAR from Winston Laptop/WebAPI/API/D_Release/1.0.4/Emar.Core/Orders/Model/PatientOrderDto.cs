using System;
using System.Collections.Generic;
using Emar.Core.Templates.Model;
using Emar.Core.Users.Model;

namespace Emar.Core.Orders.Model
{
    public class PatientOrderDto : OrderBase
    {
        /// <summary>
        /// Unique patient identifier
        /// </summary>
        public long PatientId { get; set; }

        /// <summary>
        /// Date and time the order was created.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset AddDatetime { get; set; }

        /// <summary>
        /// Date/time that the point-in-time administration was give, or
        /// Date/time that the non-point-in-time administration started
        /// </summary>
        public DateTimeOffset BeginDatetime { get; set; }

        /// <summary>
        /// Date and time the order ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? EndDatetime { get; set; }

        /// <summary>
        /// Indicates the order priority (STAT, Routine).
        /// </summary>
        public OrderPriorities Priority { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        public bool Prn { get; set; }

        /// <summary>
        /// PatientOrder status code (Pending = 1, Cancelled = 2, OnGoing = 3, OnHold = 4, PendingDiscontinue = 5, Discontinued = 6, Completed = 7, Deleted = 8)
        /// </summary>
        public OrderStatus OrderStatusCode { get; set; }

        /// <summary>
        /// PatientOrder status (Pending, Cancelled, OnGoing, OnHold, PendingDiscontinue, Discontinued, Completed, Deleted)
        /// </summary>
        public string OrderStatus
        {
            get => OrderStatusCode.ToString();

            set
            {
                if (Enum.TryParse(typeof(OrderStatus), value, out object code))
                    OrderStatusCode = (OrderStatus)code;
                else
                    OrderStatusCode = Model.OrderStatus.Pending;
            }
        }

        /// <summary>
        /// PatientOrder type (STAT, PRN, Continuous/Non-Point-In-Time, Scheduled/Point-In-Time)
        /// </summary>
        // leaving this at the Patient Level since it relies on priority, which isn't part of the remembered orders
        internal string OrderType
        {
            get
            {
                if (Priority == OrderPriorities.Stat)
                {
                    return OrderTypes.Stat.ToString();
                }

                if (Prn)
                {
                    return OrderTypes.Prn.ToString();
                }

                if (!PointInTime)
                {
                    return OrderTypes.Continuous.ToString();
                }

                return OrderTypes.Scheduled.ToString();
            }
        }

        /// <summary>
        /// Unique identifier of the user who entered the order.
        /// </summary>
        public int AddUserId { get; set; }

        /// <summary>
        /// Unique identifier of the provider who ordered the order.
        /// </summary>
        public int OrderingPhysicianId { get; set; }

        /// <summary>
        /// PatientOrder administrations.
        /// </summary>
        public IEnumerable<OrderAdministrationDto> OrderAdministrations { get; set; }

        /// <summary>
        /// PatientOrder events.
        /// </summary>
        public IEnumerable<OrderEventDto> OrderEvents { get; set; }

        //Not sure why this was setting the variable to a new list every time.
        //I changed this from a list to an IEnumerable and then gave it a typical get/set.
        //Winston Murdock, 02/05/2021.  EMAR-452.
        public IEnumerable<string> ApplicableFilters { get; set; }

        /// <summary>
        /// User who entered the order.
        /// </summary>
        public UserDto AddUser { get; set; }

        /// <summary>
        /// Provider who ordered the order.
        /// </summary>
        public UserDto OrderingPhysicianUser { get; set; }

        public DateTimeOffset? NextActionTime { get; set; }

        public IEnumerable<AvailableActionDto> AvailableActions { get; set; }

        public string? Ndc { get; set; }

        public string? PrnIndication { get; set; }
    }
}
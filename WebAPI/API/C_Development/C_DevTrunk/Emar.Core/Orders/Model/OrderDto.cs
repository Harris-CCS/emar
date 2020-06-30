using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    public class OrderDto : HateOasLinkDto
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique patient identifier
        /// </summary>
        public long PatientId { get; set; }

        /// <summary>
        /// Date and time the order was created.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; set; }

        /// <summary>
        /// Identifier for the source of this order (group ID, NDC, etc.)
        /// </summary>
        public string MedicationId { get; set; }

        /// <summary>
        /// Order type (STAT, PRN, Continuous/Non-Point-In-Time, Scheduled/Point-In-Time)
        /// </summary>
        public string OrderType
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
        /// Indicates the order priority (STAT, Routine).
        /// </summary>
        public OrderPriorities Priority { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        public bool Prn { get; set; }

        /// <summary>
        /// Indicates whether the order is Point-In-Time.
        /// </summary>
        public bool PointInTime { get; set; }

        /// <summary>
        /// Order status (Pending, Cancelled, OnGoing, OnHold, PendingDiscontinue, Discontinued, Completed)
        /// </summary>
        public string OrderStatus
        {
            get
            {
                return OrderStatusCode.ToString();
            }

            set
            {
                OrderStatusCode = (OrderStatuses)Enum.Parse(typeof(OrderStatuses), value);
            }
        }

        /// <summary>
        /// Order status code (Pending = 1, Cancelled = 2, OnGoing = 3, OnHold = 4, PendingDiscontinue = 5, Discontinued = 6, Completed = 7)
        /// </summary>
        public OrderStatuses OrderStatusCode { get; set; }

        public DateTimeOffset BeginDateTime { get; set; }

        /// <summary>
        /// Date and time the order ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; set; }

        /// <summary>
        /// Unique order frequency identifier.
        /// </summary>
        public int FrequencyId { get; set; }

        /// <summary>
        /// Unique medication route identifier.
        /// </summary>
        public int MedicationRouteId { get; set; }

        /// <summary>
        /// Order notes.
        /// </summary>
        public string OrderNotes { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value != null ? Regex.Replace(value, "( : ){2,}", " : ") : null; }
        }

        public string Unit { get; set; }

        public string Dose { get; set; }

        /// <summary>
        /// Unique identifier of the provider who ordered the order.
        /// </summary>
        public int OrderingProviderId { get; set; }

        /// <summary>
        /// Order administrations.
        /// </summary>
        public IEnumerable<OrderAdministration>? OrderAdministrations { get; set; }

        /// <summary>
        /// Order events.
        /// </summary>
        public IEnumerable<OrderEvent>? OrderEvents { get; set; }

        #region Constants
        /// <summary>
        /// Order types
        /// </summary>
        public enum OrderTypes
        {
            Stat = 1,
            Prn = 2,
            Continuous = 3,
            Scheduled = 4
        }

        /// <summary>
        /// Order priorities
        /// </summary>
        public enum OrderPriorities
        {
            Stat = 2,
            Routine = 4
        }

        /// <summary>
        /// Order statuses
        /// </summary>
        public enum OrderStatuses
        {
            Pending = 1,
            Cancelled = 2,
            OnGoing = 3,
            OnHold = 4,
            PendingDiscontinue = 5,
            Discontinued = 6,
            Completed = 7
        }
        #endregion
    }
}

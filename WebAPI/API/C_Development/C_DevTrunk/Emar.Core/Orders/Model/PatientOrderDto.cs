using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    public class PatientOrderDto : OrderBase
    {
        /// <summary>
        /// Unique patient identifier
        /// </summary>
        [Column("patient_id", TypeName = "bigint"), Required]
        public long PatientId { get; set; }

        /// <summary>
        /// Date and time the order was created.  Includes the local time timezone offset from UTC.
        /// </summary>
        [Column("create_stamp", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset CreatedDateTime { get; set; }

        /// <summary>
        /// Date/time that the point-in-time administration was give, or
        /// Date/time that the non-point-in-time administration started
        /// </summary>
        public DateTimeOffset BeginDateTime { get; set; }

        /// <summary>
        /// Date and time the order ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? EndDateTime { get; set; }

        /// <summary>
        /// Indicates the order priority (STAT, Routine).
        /// </summary>
        [Column("priority", TypeName = "tinyint"), Required]
        public OrderPriorities Priority { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        [Column("prn", TypeName = "bit"), Required]
        public bool Prn { get; set; }

        /// <summary>
        /// PatientOrder status code (Pending = 1, Cancelled = 2, OnGoing = 3, OnHold = 4, PendingDiscontinue = 5, Discontinued = 6, Completed = 7)
        /// </summary>
        public OrderStatuses OrderStatusCode { get; set; }

        /// <summary>
        /// PatientOrder status (Pending, Cancelled, OnGoing, OnHold, PendingDiscontinue, Discontinued, Completed)
        /// </summary>
        public string OrderStatus
        {
            get => OrderStatusCode.ToString();

            set
            {
                if (Enum.TryParse(typeof(OrderStatuses), value, out object code))
                    OrderStatusCode = (OrderStatuses)code;
                else
                    OrderStatusCode = OrderStatuses.Pending;
            }
        }

        /// <summary>
        /// PatientOrder type (STAT, PRN, Continuous/Non-Point-In-Time, Scheduled/Point-In-Time)
        /// </summary>
        // leaving this at the Patient Level since it relies on priority, which isn't part of the remembered orders
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
        /// Unique identifier of the provider who ordered the order.
        /// </summary>
        [NotMapped]
        public int OrderingProviderId { get; set; }

        /// <summary>
        /// PatientOrder administrations.
        /// </summary>
        [NotMapped]
        public IEnumerable<OrderAdministration>? OrderAdministrations { get; set; }

        /// <summary>
        /// PatientOrder events.
        /// </summary>
        [NotMapped]
        public IEnumerable<OrderEvent>? OrderEvents { get; set; }

        public IEnumerable<string> ApplicableFilters = new List<string>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    /// <summary>
    /// An order with its fields
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        [Column("id", TypeName = "bigint"), Key]
        public long Id { get; set; }

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
        /// Identifier for the source of this order (group ID, NDC, etc.)
        /// </summary>
        [Column("medication_id", TypeName = "varchar(50)"), Required]
        public string MedicationId { get; set; }

        /// <summary>
        /// Order type (STAT, PRN, Continuous/Non-Point-In-Time, Scheduled/Point-In-Time)
        /// </summary>
        [NotMapped]
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
        [Column("priority", TypeName = "tinyint"), Required]
        public OrderPriorities Priority { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        [Column("prn", TypeName = "bit"), Required]
        public bool Prn { get; set; }

        /// <summary>
        /// Indicates whether the order is Point-In-Time.
        /// </summary>
        [Column("point_in_time", TypeName = "bit"), Required]
        public bool PointInTime { get; set; }

        /// <summary>
        /// Order status (Pending, Cancelled, OnGoing, OnHold, PendingDiscontinue, Discontinued, Completed)
        /// </summary>
        [Column("order_status", TypeName = "varchar(10)"), Required]
        public string OrderStatus
        {
            get
            {
                return OrderStatusCode.ToString();
            }

            set
            {
                if (Enum.TryParse(typeof(OrderStatuses), value, out object code))
                {
                    OrderStatusCode = (OrderStatuses)code;
                }
            }
        }

        /// <summary>
        /// Order status code (Pending = 1, Cancelled = 2, OnGoing = 3, OnHold = 4, PendingDiscontinue = 5, Discontinued = 6, Completed = 7)
        /// </summary>
        [NotMapped]
        public OrderStatuses OrderStatusCode { get; set; }

        [Column("begin_stamp", TypeName = "datetimeoffset"), Required]
        public DateTimeOffset BeginDateTime { get; set; }

        /// <summary>
        /// Date and time the order ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        [Column("end_stamp", TypeName = "datetimeoffset")]
        public DateTimeOffset? EndDateTime { get; set; }

        /// <summary>
        /// Unique order frequency identifier.
        /// </summary>
        [Column("frequency_id", TypeName = "int"), Required]
        public int FrequencyId { get; set; }

        /// <summary>
        /// Unique medication route identifier.
        /// </summary>
        [Column("medication_route_id", TypeName = "int"), Required]
        public int MedicationRouteId { get; set; }

        /// <summary>
        /// Order notes.
        /// </summary>
        [Column("order_notes", TypeName = "ntext")]
        public string OrderNotes { get; set; }

        private string _name;
        [NotMapped]
        public string Name
        {
            get { return _name; }
            set { _name = value != null ? Regex.Replace(value, "( : ){2,}", " : ") : null; }
        }

        [NotMapped]
        public string Unit { get; set; }

        [NotMapped]
        public string Dose { get; set; }

        /// <summary>
        /// Unique identifier of the provider who ordered the order.
        /// </summary>
        [NotMapped]
        public int OrderingProviderId { get; set; }

        /// <summary>
        /// Order administrations.
        /// </summary>
        [NotMapped]
        public IEnumerable<OrderAdministration>? OrderAdministrations { get; set; }

        /// <summary>
        /// Order events.
        /// </summary>
        [NotMapped]
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

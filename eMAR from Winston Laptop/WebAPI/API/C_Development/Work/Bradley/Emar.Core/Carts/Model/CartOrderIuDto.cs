using System;
using System.Collections.Generic;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.Users.Model;

namespace Emar.Core.Carts.Model
{
    public class CartOrderIuDto : OrderIuBase
    {
        /// <summary>
        /// Unique patient identifier
        /// </summary>
        public long PatientId { get; set; }

        /// <summary>
        /// Unique identifier of the provider who entered the order in the cart.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Date and time the order was entered in the cart.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset AddDatetime { get; set; }

        /// <summary>
        /// Indicates the order priority (STAT, Routine).
        /// </summary>
        public OrderPriorities Priority { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        public bool Prn { get; set; }

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
        /// 
        /// </summary>
        public long? UserQuickListItemId { get; set; }

        /// <summary>
        /// Cart order administrations.
        /// </summary>
        public IEnumerable<CartOrderAdministrationDto>? CartOrderAdministrations { get; set; }

        public virtual ICollection<OrderInteractionDto>? OrderInteractions { get; set; }

        public virtual ICollection<AllergyReactionViewDto>? AllergyReactions { get; set; }

        public string? Ndc { get; set; }

        public string? PrnIndication { get; set; }
    }
}
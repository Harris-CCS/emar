using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Emar.Core.Orders.Model;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Model
{
    public class CartOrderDto 
    {
        /// <summary>
        /// Unique cart order identifier
        /// </summary>
        public long Id { get; set; }

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

        string ndc;
        /// <summary>
        /// National Drug Code value
        /// </summary>
        public string Ndc
        {
            get => ndc?.Trim();
            set => ndc = value?.Trim() != null ? Regex.Replace(value, "( : ){2,}", " : ") : null;
        }

        string drugId;
        /// <summary>
        /// Link to the Medication Provider Database
        /// </summary>
        public string DrugId
        {
            get => drugId?.Trim();
            set => drugId = value?.Trim() != null ? Regex.Replace(value, "( : ){2,}", " : ") : null;
        }

        string brandName;
        /// <summary>
        /// Brand name of the medication
        /// </summary>
        public string BrandName
        {
            get => brandName?.Trim();
            set => brandName = value?.Trim() != null ? Regex.Replace(value, "( : ){2,}", " : ") : null;
        }

        public decimal? Dose { get; set; }

        string doseUnit;
        public string DoseUnit
        {
            get => doseUnit?.Trim();
            set => doseUnit = value?.Trim();
        }

        /// <summary>
        /// Unique medication route identifier.
        /// </summary>
        public int? MedicationRouteId { get; set; }

        /// <summary>
        /// Medication Route.
        /// </summary>
        public MedicationRoute? MedicationRoute { get; set; }

        /// <summary>
        /// Indicates the order priority (STAT, Routine).
        /// </summary>
        public OrderPriorities Priority { get; set; }

        /// <summary>
        /// Unique order frequency identifier.
        /// </summary>
        public int? FrequencyId { get; set; }

        /// <summary>
        /// Indicates whether the order is PRN.
        /// </summary>
        public bool Prn { get; set; }

        /// <summary>
        /// Indicates whether the order is Point-In-Time.
        /// </summary>
        // Will be derivable from the Frequency - in the future,
        // include a Frequency object instead of an Id and trash this property
        public bool PointInTime { get; set; }

        /// <summary>
        /// Date/time that the point-in-time administration was give, or
        /// Date/time that the non-point-in-time administration started
        /// </summary>
        public DateTimeOffset BeginDatetime { get; set; }

        /// <summary>
        /// Date and time the order ended.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset? EndDatetime { get; set; }

        string orderNotes;
        /// <summary>
        /// Order notes.
        /// </summary>
        public string OrderNotes
        {
            get => orderNotes?.Trim();
            set => orderNotes = value?.Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        public long? UserQuickListItemId { get; set; }

        /// <summary>
        /// Cart order administrations.
        /// </summary>
        public IEnumerable<CartOrderAdministration>? CartOrderAdministrations { get; set; }

        /// <summary>
        /// Provider who entered the order in the cart.
        /// </summary>
        public User User { get; set; }
    }
}
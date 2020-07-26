using System.Text.RegularExpressions;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Model
{
    public class OrderBase
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

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
        /// Unique identifier of the Medication Route
        /// </summary>
        public int? MedicationRouteId { get; set; }

        /// <summary>
        /// Medication Route.
        /// </summary>
        public MedicationRoute MedicationRoute { get; set; }

        /// <summary>
        /// Unique order frequency identifier.
        /// </summary>
        public int? FrequencyId { get; set; }

        /// <summary>
        /// Indicates whether the order is Point-In-Time.
        /// </summary>
        // Will be derivable from the Frequency - in the future,
        // include a Frequency object instead of an Id and trash this property
        public bool PointInTime { get; set; }

        string orderNotes;
        /// <summary>
        /// Order notes.
        /// </summary>
        public string OrderNotes
        {
            get => orderNotes?.Trim();
            set => orderNotes = value?.Trim();
        }
    }

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
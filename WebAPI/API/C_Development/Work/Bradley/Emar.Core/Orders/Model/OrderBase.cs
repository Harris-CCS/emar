using System.Text.RegularExpressions;
using Emar.Core.Medications.Model;

namespace Emar.Core.Orders.Model
{
    public class OrderBase
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

        internal string DateFormat { get; set; } = "MM/dd/yyyy";
        internal string TimeFormat { get; set; } = "HH:mm";

        //string _ndc;
        ///// <summary>
        ///// National Drug Code value
        ///// </summary>
        //public string Ndc
        //{
        //    get => _ndc?.Trim();
        //    set => _ndc = value?.Trim() != null ? Regex.Replace(value, "( : ){2,}", " : ") : null;
        //}

        //string _drugId;
        ///// <summary>
        ///// Link to the Medication Provider Database
        ///// </summary>
        //public string DrugId
        //{
        //    get => _drugId?.Trim();
        //    set => _drugId = value?.Trim() != null ? Regex.Replace(value, "( : ){2,}", " : ") : null;
        //}

        //string _brandName;
        ///// <summary>
        ///// Brand name of the medication
        ///// </summary>
        //public string BrandName
        //{
        //    get => _brandName?.Trim();
        //    set => _brandName = value?.Trim() != null ? Regex.Replace(value, "( : ){2,}", " : ") : null;
        //}

        internal int MedicationId { get; set; }

        public MedicationDto Medication { get; set; }

        public decimal? Dose { get; set; }

        internal int? MedicationUnitId { get; set; }
        public MedicationUnitDto DoseUnit { get; set; }

        internal int? MedicationRouteId { get; set; }
        /// <summary>
        /// DTO of the Medication Route
        /// </summary>
        public MedicationRouteDto MedicationRoute { get; set; }
        
        internal int? FrequencyId { get; set; }
        /// <summary>
        /// DTO of the Frequency Schedule
        /// </summary>
        public FrequencyScheduleDto FrequencySchedule { get; set; }

        /// <summary>
        /// Indicates whether the order is Point-In-Time.
        /// </summary>
        // Will be derivable from the Frequency - in the future,
        // include a Frequency object instead of an Id and trash this property
        public bool PointInTime { get; set; }

        string _orderNotes;
        /// <summary>
        /// Order notes.
        /// </summary>
        public string OrderNotes
        {
            get => _orderNotes?.Trim();
            set => _orderNotes = value?.Trim();
        }
    }
}
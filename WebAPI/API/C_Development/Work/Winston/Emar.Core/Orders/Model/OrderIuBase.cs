using System.Text.RegularExpressions;

namespace Emar.Core.Orders.Model
{
    public class OrderIuBase
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

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

        public int MedicationId { get; set; }

        public decimal? Dose { get; set; }

        public int? MedicationUnitId { get; set; }
        
        public int? MedicationRouteId { get; set; }

        public int? FrequencyId { get; set; }

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
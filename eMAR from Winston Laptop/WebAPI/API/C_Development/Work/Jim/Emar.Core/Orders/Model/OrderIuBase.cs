namespace Emar.Core.Orders.Model
{
    public class OrderIuBase
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

        public int MedicationId { get; set; }

        public decimal? Dose { get; set; }

        public int? MedicationUnitId { get; set; }

        public int? MedicationRouteId { get; set; }

        public int? FrequencyId { get; set; }

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

        public int? UserQuickListItemId { get; set; }

        public int? AntimicrobialIndicationId { get; set; }

        public string AntimicrobialIndicationText { get; set; }

        public long? PatientProblemId { get; set; }

        public int? Duration { get; set; }

        public int? DurationUnitId { get; set; }

        public DurationUnitDto DurationUnit { get; set; }
    }
}
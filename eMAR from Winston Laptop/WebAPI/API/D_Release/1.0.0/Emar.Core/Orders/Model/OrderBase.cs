using System.Collections.Generic;
using System.Collections.ObjectModel;
using Emar.Core.Medications.Model;
using Emar.Core.Patients.Model;

namespace Emar.Core.Orders.Model
{
    public class OrderBase
    {
        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long Id { get; set; }

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

        internal int? AntimicrobialIndicationId { get; set; }

        public AntimicrobialIndicationDto AntimicrobialIndication { get; set; }

        public string AntimicrobialIndicationText { get; set; }

        internal long? PatientProblemId { get; set; }

        public PatientProblemDto PatientProblem { get; set; }

        public int? Duration { get; set; }

        public int? DurationUnitId { get; set; }

        public DurationUnitDto DurationUnit { get; set; }


        public virtual ICollection<OrderInteractionDto> OrderInteractions { get; set; }
        public virtual ICollection<AllergyReactionViewDto> AllergyReactions { get; set; }


        internal void AddOrderInteraction(OrderInteractionDto orderInteractionDto)
        {
            OrderInteractions ??= new Collection<OrderInteractionDto>();
            OrderInteractions.Add(orderInteractionDto);
        }

        internal void AddAllergyReaction(AllergyReactionViewDto allergyReactionViewDto)
        {
            AllergyReactions ??= new Collection<AllergyReactionViewDto>();
            AllergyReactions.Add(allergyReactionViewDto);
        }
    }
}
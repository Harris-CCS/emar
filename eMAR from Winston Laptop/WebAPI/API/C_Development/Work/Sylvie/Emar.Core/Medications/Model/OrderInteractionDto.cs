using System.Collections.Generic;

namespace Emar.Core.Medications.Model
{
    public class OrderInteractionDto
    {
        public long Id { get; set; }
        public long MedicationInteractionId { get; set; }
        public byte DrugNum { get; set; }
        public long? PatientOrderId { get; set; }
        public long? PatientCartOrderId { get; set; }
        public long? PatientHomeMedicationId { get; set; }

        public DrugInteractionViewDto DrugInteraction { get; set; }
    }
    public class OrderInteractionDtoComparer : IEqualityComparer<OrderInteractionDto>
    {
        public bool Equals(OrderInteractionDto x, OrderInteractionDto y)
        {
            return
                x.DrugInteraction?.InteractionOrderName == y.DrugInteraction?.InteractionOrderName &&
                x.DrugInteraction?.Severity == y.DrugInteraction?.Severity;
        }

        public int GetHashCode(OrderInteractionDto obj)
        {
            return obj.DrugInteraction == null ? 0.GetHashCode() :
                obj.DrugInteraction.InteractionOrderName.GetHashCode() ^
                obj.DrugInteraction.Severity.GetHashCode();
        }
    }
}
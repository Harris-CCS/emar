namespace Emar.Core.Medications.Model
{
    public  class OrderInteractionDto
    {
        public long Id { get; set; }
        public long MedicationInteractionId { get; set; }
        public byte DrugNum { get; set; }
        public long? PatientOrderId { get; set; }
        public long? PatientCartOrderId { get; set; }
        public long? PatientHomeMedicationId { get; set; }

        public DrugInteractionViewDto DrugInteraction { get; set; }
    }
}
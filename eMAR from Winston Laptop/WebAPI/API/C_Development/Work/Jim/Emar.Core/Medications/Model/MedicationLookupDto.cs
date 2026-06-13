namespace Emar.Core.Medications.Model
{
    public class MedicationLookupDto
    {
        public int MedicationId { get; internal set; }

        public string BrandName { get; internal set; }

        public string DrugId { get; internal set; }

        public byte InpatientMatch { get; internal set; }

        public byte OutpatientMatch { get; internal set; }

        public byte PyxisMatch { get; internal set; }

        public decimal Medid { get; set; }

        public decimal GcnSeqNo { get; set; }

        public decimal HiclSeqNo { get; set; }
    }
}
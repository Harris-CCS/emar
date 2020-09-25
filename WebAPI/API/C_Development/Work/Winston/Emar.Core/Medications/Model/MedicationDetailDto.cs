namespace Emar.Core.Medications.Model
{
    public class MedicationDetailDto
    {
        public int Id { get; set; } 
        public int MedicationId { get; set; }
        public string DrugId { get; set; }
        public string BrandName { get; set; }
        public string ActiveList { get; set; }
        public decimal? Dose { get; set; }
        public int? MedicationUnitId { get; set; }
        public int? MedicationRouteId { get; set; }
        public bool IsActive { get; set; }
    }
}

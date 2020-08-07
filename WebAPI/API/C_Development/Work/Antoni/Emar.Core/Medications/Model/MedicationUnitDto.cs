namespace Emar.Core.Medications.Model
{
    public class MedicationUnitDto
    {
        public int Id { get; set; }
        public long SiteId { get; set; }
        public string Code { get; set; }
        public string UnitName { get; set; }
        public string PrintName { get; set; }
        public bool Active { get; set; }
    }
}
namespace Emar.Core.Orders.Model
{
    public class MedicationUnitDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string Code { get; set; }
        public string UnitName { get; set; }
        public string PrintName { get; set; }
        public bool Active { get; set; }
    }
}
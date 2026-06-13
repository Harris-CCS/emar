namespace Emar.Core.Medications.Model
{
    public class OrderInstructionDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
namespace Emar.Core.Medications.Model
{
    public class FrequencyScheduleDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string ScheduleName { get; set; }
        public bool PointInTime { get; set; }
        public string Notes { get; set; }
    }
}
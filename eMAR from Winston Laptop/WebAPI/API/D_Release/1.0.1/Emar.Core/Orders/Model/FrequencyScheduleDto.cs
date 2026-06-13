namespace Emar.Core.Orders.Model
{
    public class FrequencyScheduleDto
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public string ScheduleName { get; set; }
        public bool PointInTime { get; set; }
        public string Notes { get; set; }
        public bool? Prn { get; set; }
        public FrequencyTypeDto FrequencyType { get; set; }
    }
}
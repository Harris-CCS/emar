namespace PulseCheck.Domain
{
    public class SiteRule
    {
        public long Id { get; set; }
        public long SiteId { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public bool? BoolValue { get; set; }
    }
}
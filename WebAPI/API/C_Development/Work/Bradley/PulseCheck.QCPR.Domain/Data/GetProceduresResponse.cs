namespace PulseCheck.QCPR.Domain.Data
{
    public class GetProceduresResponse
    {
        public int SiteId { get; set; }

        public string Code { get; set; }

        public string Facility { get; set; }

        public string Interface { get; set; }

        public string Name { get; set; }

        public long Id { get; set; }
    }
}
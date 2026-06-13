namespace Emar.Core.ExternalIds.Model
{
    public class ExternalIdDto
    {
        public long InternalId { get; set; }

        public string Vendor { get; set; }

        public string Entity { get; set; }

        public string ExternalId { get; set; }
    }
}
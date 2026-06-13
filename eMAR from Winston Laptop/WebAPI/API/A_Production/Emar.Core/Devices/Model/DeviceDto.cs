namespace Emar.Core.Devices.Model
{
    public class DeviceDto
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string PrintQueueName { get; set; }

        public string Tray { get; set; }

        public string DeviceType { get; set; }

        public string PclType { get; set; }

        public bool IsLastUsed { get; set; }
    }
}

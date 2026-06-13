using System.Linq;
using Emar.Core.Sites.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Devices.Model.Mappings
{
    public static class DeviceMapper
    {
        public static DeviceDto MapDevice(Device device)
        {
            if (device == null)
            {
                return null;
            }

            var ret = new DeviceDto
            {
                Id = device.Id,
                SiteId = device.SiteId,
                Address = device.Address,
                Description = device.Description,
                IsActive = device.IsActive,
                PrintQueueName = device.PrintQueueName,
                Tray = device.Tray,
                DeviceType = device.DeviceType,
                PclType = device.PclType
            };

            return ret;
        }
    }
}

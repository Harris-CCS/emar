using Emar.Core.Devices.Model;
using System.Collections.Generic;

namespace Emar.Core.Devices.Repository
{
    public interface IDeviceRepository
    {
        IEnumerable<DeviceDto> GetDevices(int siteId, int userId);
    }
}

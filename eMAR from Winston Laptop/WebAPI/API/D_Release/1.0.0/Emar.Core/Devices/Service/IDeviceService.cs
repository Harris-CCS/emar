using Emar.Core.Devices.Model;
using System.Collections.Generic;

namespace Emar.Core.Devices.Service
{
    public interface IDeviceService
    {
        IEnumerable<DeviceDto> GetDevices(int siteId, int userId);
    }
}

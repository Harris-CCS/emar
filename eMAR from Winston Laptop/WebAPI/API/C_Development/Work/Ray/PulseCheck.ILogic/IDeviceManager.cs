using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.ILogic
{
    public interface IDeviceManager
    {
        Task<MobileDevice> GetDeviceByIdAsync(string deviceId);
        Task<int> Save(MobileDevice device);
        Task<List<MobileDevice>> GetDevices();
        Task<bool> CheckDeviceAuthorization(string activationCode);
        Task DeleteDevice(string deviceId);
        void AddDevice(MobileDevice device);
        string CreateAuthorizationCode();
    }
}

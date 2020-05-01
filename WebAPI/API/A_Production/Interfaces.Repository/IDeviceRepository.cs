using System.Threading.Tasks;
using DomainModel;
using System;
using System.Collections.Generic;

namespace Interfaces.Repository
{
    public interface IDeviceRepository
    {
        Task<MobileDevice> GetDeviceByIdAsync(string deviceId);
        Task<int> Save(MobileDevice device);
        Task<List<MobileDevice>> GetDevices();
        Task<bool> CheckDeviceAuthorization(string activationCode);
        Task DeleteDevice(string deviceId);
        void AddMobileDevice(MobileDevice device);
        string CreateAuthorizationCode();
    }
}

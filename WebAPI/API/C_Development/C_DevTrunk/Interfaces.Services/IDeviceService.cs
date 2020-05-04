using System.Threading.Tasks;
using DomainModel;
using System;
using System.Collections.Generic;

namespace Interfaces.Services
{
    public interface IDeviceService
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

using System.Threading.Tasks;
using DomainModel;
using Interfaces.Repository;
using Interfaces.Services;
using System.Collections.Generic;

namespace Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;

        /// <summary>
        /// Device service constructor
        /// </summary>
        /// <param name="deviceRepository">IDeviceRepository instance</param>
        public DeviceService(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        /// <summary>
        /// Get a device by ID
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>MobileDevice object</returns>
        public async Task<MobileDevice> GetDeviceByIdAsync(string deviceId)
        {
            return await _deviceRepository.GetDeviceByIdAsync(deviceId);
        }

        public async Task<List<MobileDevice>> GetDevices()
        {
            return await _deviceRepository.GetDevices();
        }

        /// <summary>
        /// Save changes to a MobileDevice
        /// </summary>
        /// <param name="device">MobileDevice object</param>
        /// <returns>int result from save</returns>
        public async Task<int> Save(MobileDevice device)
        {
            return await _deviceRepository.Save(device);
        }

        public async Task<bool> CheckDeviceAuthorization(string activationCode)
        {
            var isActive = await _deviceRepository.CheckDeviceAuthorization(activationCode);
            return isActive;
        }

        public void AddDevice(MobileDevice device)
        {
            _deviceRepository.AddMobileDevice(device);
        }

        public string CreateAuthorizationCode()
        {
            return  _deviceRepository.CreateAuthorizationCode();
        }

        public async Task DeleteDevice(string deviceId)
        {
            await _deviceRepository.DeleteDevice(deviceId);
        }
    }
}

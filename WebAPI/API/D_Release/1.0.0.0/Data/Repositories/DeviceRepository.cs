using System.Data.Entity;
using System.Threading.Tasks;
using DomainModel;
using Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Repositories
{
    /// <summary>
    /// Device repository
    /// </summary>
    public class DeviceRepository : BaseRepository, IDeviceRepository
    {
        /// <summary>
        /// Defaultc constructor
        /// </summary>
        /// <param name="context">Database context</param>
        public DeviceRepository(IbexContext context) : base(context)
        {

        }

        /// <summary>
        /// Get a device by ID
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>MobileDevice object</returns>
        public async Task<MobileDevice> GetDeviceByIdAsync(string deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
        }

        /// <summary>
        /// Get a list of devices
        /// </summary>
        /// <returns>List of MobileDevice objects</returns>
        public async Task<List<MobileDevice>> GetDevices()
        {
           return await _context.Devices.ToListAsync();
        }
        
        /// <summary>
        /// Save changes made to a MobileDevice
        /// </summary>
        /// <param name="device">MobileDevice object</param>
        /// <returns>int success value from SaveChangesAsync in context</returns>
        public async Task<int> Save(MobileDevice device)
        {
            _context.Entry(device).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckDeviceAuthorization(string activationCode)
        {
            var result = await _context.CheckAuthorizationCode(activationCode).FirstOrDefaultAsync();
            return result != null && result.IsActive;
        }

        public void AddMobileDevice(MobileDevice device)
        {
            _context.AddDevice(device.DeviceId, device.OS, device.OSVersion, device.Manufacturer, device.Model, device.FriendlyName);
        }

        public string CreateAuthorizationCode()
        {            
            return _context.CreateAuthorizationCode().FirstOrDefault();
        }

        public async Task DeleteDevice(string deviceId)
        {
             _context.DeleteDevice(deviceId);
        }
    }
}

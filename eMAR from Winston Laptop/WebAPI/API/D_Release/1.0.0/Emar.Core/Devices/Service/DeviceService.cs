using Emar.Core.Devices.Model;
using Emar.Core.Devices.Repository;
using System.Collections.Generic;

namespace Emar.Core.Devices.Service
{
    public partial class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;

        public DeviceService(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        //TODO: Accept user_id as a parameter here (and in the interface).
        public IEnumerable<DeviceDto> GetDevices(int siteId, int userId)
        {
            //Call to the repository to get the data from the DB.
            var devices = _deviceRepository.GetDevices(siteId, userId);

            //The abvove should return us a list of DeviceDTO objects, so there's no need to call out to the mapper here.
            //We do the mapping in the repository, becasue it has a class with an extra field that we need.

            ////List of DTO objects.
            //List<DeviceDto> devicesDtos = new List<DeviceDto>();

            ////For each device in the list, map it to a DTO object and add to the DTO list.
            //foreach (Device device in devices)
            //{
            //    devicesDtos.Add(DeviceMapper.MapDevice(device));
            //}

            ////Return.
            return devices;
        }
    }
}

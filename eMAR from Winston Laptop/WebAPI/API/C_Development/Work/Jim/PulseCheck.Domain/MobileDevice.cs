using System;

namespace DomainModel
{
    /// <summary>
    /// Class to represent a user's mobile device
    /// </summary>
    public class MobileDevice
    {
        /// <summary>
        /// Unique DB identifier for device
        /// </summary>
        public int MobileDeviceId { get; set; }

        /// <summary>
        /// Unique client identifier for device
        /// </summary>
        public Guid DeviceUUID { get; set; }

        /// <summary>
        /// Device manufacturer string
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Device model string
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Device operating system string
        /// </summary>
        public string OS { get; set; }

        /// <summary>
        /// Device operating system version string
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// Device "friendly name" string - name provided by user
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Boolean flag for whether this device has been authorized
        /// </summary>
        public bool IsAuthorized { get; set; }
    }
}
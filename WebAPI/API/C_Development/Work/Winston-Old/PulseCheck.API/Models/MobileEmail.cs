using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PulseCheck.API.Models
{
    /// <summary>
    /// Modeul for sending e-mails from the API
    /// </summary>
    public class MobileEmail
    {
        /// <summary>
        /// Recipient's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Recipient's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// "Temporary" account password
        /// </summary>
        public string TempPassword { get; set; }
        /// <summary>
        /// API url
        /// </summary>
        public string APIUrl { get; set; }
        /// <summary>
        /// Passcode for authorizing a device
        /// </summary>
        public string DevicePasscode { get; set; }
        /// <summary>
        /// Subject of the e-mail
        /// </summary>
        public string Subject { get; set; }
    }
}
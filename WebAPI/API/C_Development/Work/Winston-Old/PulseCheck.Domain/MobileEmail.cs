namespace PulseCheck.Domain
{
    /// <summary>
    /// Modeul for sending e-mails from the API
    /// </summary>
    public class MobileEmail
    {
        /// <summary>
        /// Recipient's e-mail/login
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Recipient's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Recipient's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Url for link in e-mail
        /// </summary>
        public string LinkUrl { get; set; }
        /// <summary>
        /// Url for the API
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
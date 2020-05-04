namespace PulseCheck.Domain
{
    /// <summary>
    /// An object containing a user's credential information
    /// </summary>
    public class AuthCredentials
    {
        /// <summary>
        /// Username. Could be master/API username, or Domain username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User's password. Could be master/API user's password, or Domain user's password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Domain name
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Device Identifier
        /// </summary>
        public string DeviceId { get; set; }
    }
}
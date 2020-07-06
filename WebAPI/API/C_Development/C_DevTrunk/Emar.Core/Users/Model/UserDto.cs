using System;

namespace Emar.Core.Users.Model
{
    public class UserDto 
    {
        public int Id { get; set; }
        public short SiteId { get; set; }
        public bool Active { get; set; }
        public bool InitialsDisplay { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool OrderingOnlyPhysician { get; set; }
        public bool NameDisplayPreference { get; set; }
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public byte[] Salt { get; set; }
        public DateTimeOffset LastLoginTime { get; set; }
        public int FailedLoginAttempts { get; set; }
    }
}
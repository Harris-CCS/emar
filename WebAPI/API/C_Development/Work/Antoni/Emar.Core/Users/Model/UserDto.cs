using System;
using Emar.Data.Entities;

namespace Emar.Core.Users.Model
{
    public class UserDto
    {
        public int Id { get; set; }
        public short SiteId { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
        public string InitialsDisplay { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool OrderingOnlyPhysician { get; set; }
        public bool NameDisplayInitials { get; set; }
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public byte[] Salt { get; set; }
        public DateTimeOffset LastLoginTime { get; set; }
        public int FailedLoginAttempts { get; set; }

        public string Name
        {
            get
            {
                if (NameDisplayInitials)
                {
                    return InitialsDisplay;
                }

                var firstName = (FirstName ?? "").Trim();

                if (firstName.Length == 1)
                {
                    firstName += ".";
                }

                var ret = firstName;

                ret += (ret != "" && !string.IsNullOrWhiteSpace(LastName)) ? " " : "";
                ret += (LastName ?? "").Trim();

                return ret;
            }
        }

        public Site Site { get; set; }
        public string SiteName
        {
            get
            {
                return Site.Name;
            }
        }
    }
}
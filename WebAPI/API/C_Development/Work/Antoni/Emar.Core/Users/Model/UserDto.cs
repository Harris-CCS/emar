using System;
using System.IO;
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
        public DateTimeOffset? LastLoginTime { get; set; }
        public int FailedLoginAttempts { get; set; }

        public string Name
        {
            get
            {
                if (NameDisplayInitials)
                {
                    return InitialsDisplay;
                }

                return ((FirstName ?? String.Empty) + @" " + (LastName ?? String.Empty)).Trim();
            }
        }

        public Site Site { get; set; }
        public string SiteName
        {
            get => Site?.Name;
        }
    }
}
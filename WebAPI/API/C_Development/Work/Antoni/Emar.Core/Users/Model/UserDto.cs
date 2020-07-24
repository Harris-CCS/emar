using System;
using System.IO;
using Emar.Data.Entities;

namespace Emar.Core.Users.Model
{
    public class UserDto
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        string type;
        public string Type
        {
            get => type?.Trim();
            set => type = value?.Trim();
        }

        public bool IsActive { get; set; }

        string initialsDisplay;
        public string InitialsDisplay
        {
            get => initialsDisplay?.Trim();
            set => initialsDisplay = value?.Trim();
        }

        string firstName;
        public string FirstName
        {
            get => firstName?.Trim();
            set => firstName = value?.Trim();
        }

        string lastName;
        public string LastName
        {
            get => lastName?.Trim();
            set => lastName = value?.Trim();
        }

        public bool OrderingOnlyPhysician { get; set; }

        public bool NameDisplayInitials { get; set; }

        string loginName;
        public string LoginName
        {
            get => loginName?.Trim();
            set => loginName = value?.Trim();
        }

        string loginPassword;
        public string LoginPassword
        {
            get => loginPassword?.Trim();
            set => loginPassword = value?.Trim();
        }

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

                return ((FirstName ?? String.Empty) + @" " + (LastName ?? String.Empty));
            }
        }

        public Site Site { get; set; }

        public string SiteName
        {
            get => Site?.Name;
        }
    }
}
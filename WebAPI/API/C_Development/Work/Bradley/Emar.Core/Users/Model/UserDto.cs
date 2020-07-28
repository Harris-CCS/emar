using System;
using System.IO;
using Emar.Core.Sites.Model;
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

        string middleName;
        public string MiddleName
        {
            get => middleName?.Trim();
            set => middleName = value?.Trim();
        }

        string lastName;
        public string LastName
        {
            get => lastName?.Trim();
            set => lastName = value?.Trim();
        }

        string nameSuffix;
        public string NameSuffix
        {
            get => nameSuffix?.Trim();
            set => nameSuffix = value?.Trim();
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

        public SiteDto Site { get; set; }

        public string SiteName
        {
            get => Site?.Name;
        }
    }
}
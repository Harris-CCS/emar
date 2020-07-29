using System;
using Emar.Core.Sites.Model;

namespace Emar.Core.Users.Model
{
    public class UserDto
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        string _type;
        public string Type
        {
            get => _type?.Trim();
            set => _type = value?.Trim();
        }

        public bool IsActive { get; set; }

        string _initialsDisplay;
        public string InitialsDisplay
        {
            get => _initialsDisplay?.Trim();
            set => _initialsDisplay = value?.Trim();
        }

        string _firstName;
        public string FirstName
        {
            get => _firstName?.Trim();
            set => _firstName = value?.Trim();
        }

        string _middleName;
        public string MiddleName
        {
            get => _middleName?.Trim();
            set => _middleName = value?.Trim();
        }

        string _lastName;
        public string LastName
        {
            get => _lastName?.Trim();
            set => _lastName = value?.Trim();
        }

        string _nameSuffix;
        public string NameSuffix
        {
            get => _nameSuffix?.Trim();
            set => _nameSuffix = value?.Trim();
        }

        public bool OrderingOnlyPhysician { get; set; }

        public bool NameDisplayInitials { get; set; }

        string _loginName;
        public string LoginName
        {
            get => _loginName?.Trim();
            set => _loginName = value?.Trim();
        }

        string _loginPassword;
        public string LoginPassword
        {
            get => _loginPassword?.Trim();
            set => _loginPassword = value?.Trim();
        }

        public byte[] Salt { get; set; }

        public DateTimeOffset? LastLoginTime { get; set; }

        public int FailedLoginAttempts { get; set; }

        string _displayName;
        public string DisplayName
        {
            get => _displayName?.Trim();
            set => _displayName = value?.Trim();
        }

        public SiteDto Site { get; set; }

        public string SiteName
        {
            get => Site?.Name;
        }
    }
}
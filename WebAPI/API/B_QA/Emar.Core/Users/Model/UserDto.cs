using Emar.Core.Helpers;
using Emar.Core.Sites.Model;

namespace Emar.Core.Users.Model
{
    public class UserDto
    {
        public int Id { get; set; }

        string _type;
        public string TypeCode
        {
            get => _type?.Trim();
            set => _type = value?.Trim();
        }

        public string TypeDescription
        {
            get
            {
                switch (_type)
                {
                    case "D":
                        return "physician";
                    case "N":
                        return "nurse";
                    case "S":
                        return "associate";
                    case "A":
                        return "administrator";
                    default:
                        return "unknown";
                }
            }
        }

        //public bool IsActive { get; set; }

        string _initialsDisplay;
        public string UserInitials
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

        public string DisplayName => NameHelper.GetDisplayName(FirstName, MiddleName, LastName, NameSuffix);

        public bool OrderingOnlyPhysician { get; set; }

        public bool DisplayInitialsIndicator { get; set; }

        /* Commenting out the following so that sensitive information is not
         * sent over the wire needlessly */
        //string _loginName;
        //public string LoginName
        //{
        //    get => _loginName?.Trim();
        //    set => _loginName = value?.Trim();
        //}

        //string _loginPassword;
        //public string LoginPassword
        //{
        //    get => _loginPassword?.Trim();
        //    set => _loginPassword = value?.Trim();
        //}

        //public byte[] Salt { get; set; }

        //public DateTimeOffset? LastLoginTime { get; set; }

        //public int FailedLoginAttempts { get; set; }

        public SiteDto Site { get; set; }
    }
}
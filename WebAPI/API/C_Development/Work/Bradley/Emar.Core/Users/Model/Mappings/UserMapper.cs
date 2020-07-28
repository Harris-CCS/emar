using Emar.Core.Sites.Model.Mappings;
using Emar.Data.Entities;

namespace Emar.Core.Users.Model.Mappings
{
    public static class UserMapper
    {
        public static UserDto MapUser(User user)
        {
            if (user == null)
            {
                return null;
            }

            UserDto userDto = new UserDto
            {
                Id = user.Id,
                SiteId = user.SiteId,
                Type = user.Type,
                IsActive = user.IsActive,
                DisplayName = GetDisplayName(user),
                InitialsDisplay = user.InitialsDisplay,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                NameSuffix = user.NameSuffix,
                OrderingOnlyPhysician = user.OrderingOnlyPhysician ?? false,
                NameDisplayInitials = user.NameDisplayInitials ?? false,
                LoginName = user.LoginName,
                LoginPassword = user.LoginPassword,
                Salt = user.Salt,
                LastLoginTime = user.LastLoginTime,
                FailedLoginAttempts = user.FailedLoginAttempts,
                Site = SiteMapper.MapSite(user.Site)
            };

            return userDto;
        }

        public static UserMinimalDto MapUserMinimal(User user)
        {
            if (user == null)
            {
                return null;
            }

            UserMinimalDto userDto = new UserMinimalDto
            {
                Id = user.Id,
                SiteName = user.Site.Name,
                DisplayName = GetDisplayName(user),
            };

            return userDto;
        }

        private static string GetDisplayName(User user)
        {
            if (user.NameDisplayInitials ?? false)
            {
                return user.InitialsDisplay;
            }

            var firstName = (user.FirstName ?? "");
            firstName += firstName.Length == 1 ? "." : "";

            var middleName = (user.MiddleName ?? "");
            middleName += middleName.Length == 1 ? "." : "";

            var lastName = (user.LastName ?? "");
            lastName += lastName.Length == 1 ? "." : "";

            var ret = firstName;
            ret += (ret != "" && middleName != "") ? " " : "";
            ret += middleName;
            ret += (ret != "" && lastName != "") ? " " : "";
            ret += lastName;
            ret += (ret != "" && !string.IsNullOrEmpty(user.NameSuffix)) ? ", " : "";
            ret += user.NameSuffix;

            return ret;
        }
    }
}
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

            UserDto _userDto = new UserDto
            {
                Id = user.Id,
                SiteId = user.SiteId,
                Type = user.Type,
                IsActive = user.IsActive,
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

            return _userDto;
        }
    }
}
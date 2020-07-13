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
                Active = user.Active,
                InitialsDisplay = user.InitialsDisplay,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OrderingOnlyPhysician = user.OrderingOnlyPhysician,
                NameDisplayInitials = user.NameDisplayInitials,
                LoginName = user.LoginName,
                LoginPassword = user.LoginPassword,
                Salt = user.Salt,
                LastLoginTime = user.LastLoginTime,
                FailedLoginAttempts = user.FailedLoginAttempts,
                Site = user.Site
            };

            return _userDto;
        }
    }
}
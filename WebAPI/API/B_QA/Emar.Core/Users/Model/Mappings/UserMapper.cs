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
                TypeCode = user.Type,
                //IsActive = user.IsActive,
                UserInitials = user.UserInitials,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                NameSuffix = user.NameSuffix,
                OrderingOnlyPhysician = user.OrderingOnlyPhysician ?? false,
                DisplayInitialsIndicator = user.DisplayInitialsIndicator ?? false,
                //LoginName = user.LoginName,
                //// commented out for security purposes ////
                //LoginPassword = user.LoginPassword,
                //Salt = user.Salt,
                //LastLoginTime = user.LastLoginTime,
                //FailedLoginAttempts = user.FailedLoginAttempts,
                Site = SiteMapper.MapSite(user.Site)
            };

            return userDto;
        }
    }
}
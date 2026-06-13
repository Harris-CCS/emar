using Emar.Core.Sites.Model.Mappings;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;
using System.Linq;

namespace Emar.Core.Users.Model.Mappings
{
    public static class UserMapper
    {
        public static UserDto MapUser(User user, IUserRepository repository = null)
        {
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                TypeCode = user.Type,
                IsActive = user.IsActive,
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
                Site = SiteMapper.MapSite(user.Site),
                UserSettings = user.UserSettings.Select(setting => NewMethod(repository, setting)).ToList()
            };
        }

        private static UserSettingDto NewMethod(IUserRepository repository, UserSetting setting)
        {
            if (repository == null)
                return null;

            var settingDto = new UserSettingDto
            {
                Id = setting.Id,
                SettingId = setting.SettingId,
                SettingDescription = repository.GetSettingString(setting.SettingId),
                SettingValue = setting.SettingValue
            };
            return settingDto;
        }
    }
}
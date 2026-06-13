using Emar.Data.Entities;
using System.Linq;
using Emar.Core.Users.Repository;

namespace Emar.Core.InboundData.Model.Mappings
{
    internal static class IdsGenericMapper
    {
        private static IUserRepository _userRepository;

        public static User MapInboundUserDataDto(int id, InboundUserDataDto dto, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            if (dto == null)
                return null;

            return new User
            {
                Id = id,
                SiteId = dto.InternalSiteId,
                Type = dto.Type,
                IsActive = dto.IsActive,
                UserInitials = dto.InitialDisplay,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                NameSuffix = dto.NameSuffix,
                OrderingOnlyPhysician = dto.OrderingOnlyPhysician,
                DisplayInitialsIndicator = dto.NameDisplayInitials,
                LoginName = dto.LoginName,
                LoginPassword = dto.LoginPassword,
                Salt = dto.Salt,
                LastLoginTime = dto.LastLoginTime,
                FailedLoginAttempts = dto.FailedLoginAttempts,
                UserSettings = dto.UserSettings
                    .Select(setting => MapInboundUserSetting(setting, dto.InternalSiteId)).ToList()
            };
        }

        private static UserSetting MapInboundUserSetting(InboundUserSettingsDto dto, int siteId)
        {
            if (dto == null) return null;

            return new UserSetting
            {
                SiteId = siteId, 
                SettingId = _userRepository.GetSettingId(dto.SettingString),
                SettingValue = dto.SettingValue
            };
        }

        public static Patient MapInboundPatientDataDto(in long patientId, InboundPatientDataDto inboundPatientDataDto)
        {
            throw new System.NotImplementedException();
        }
    }
}
using Emar.Core.InboundData.Service.IbexSpecific;
using Emar.Core.Sites.Repository;
using Emar.Data.IbexEntities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.InboundData.Model.Mappings
{
    public static class IbexInboundMapper
    {
        public static InboundUserDataDto MapUser(EmarUsersRetrieveView dbObj, ISiteRepository siteRepository,
            ILogger<IIbexIdsProcessorService> logger)
        {
            if (dbObj == null)
                return null;

            if (string.IsNullOrWhiteSpace(dbObj.LoginName))
            {
                logger.LogWarning(
                    $"Skipping user #{dbObj.Id} ({dbObj.LastName}, {dbObj.FirstName}) because the user has no login_name ([ibex].[dbo].[drs].[loginid])");
                return null;
            }

            int siteId = siteRepository.GetInternalSiteId(dbObj.SiteId);
            var siteTimeZone = siteRepository.GetSiteTimeZone(siteId);

            var ret = new InboundUserDataDto
            {
                ExternalId = dbObj.Id.ToString(),
                ExternalUserNum = dbObj.Id,
                InternalSiteId = siteId,
                Type = dbObj.Type,
                IsActive = dbObj.IsActive == 1,
                InitialDisplay = dbObj.InitialsDisplay,
                FirstName = dbObj.FirstName,
                LastName = dbObj.LastName,
                MiddleName = dbObj.MiddleName,
                NameSuffix = dbObj.NameSuffix,
                OrderingOnlyPhysician = dbObj.OrderingOnlyPhysician == 1,
                NameDisplayInitials = dbObj.NameDisplayInitials == 1,
                LoginName = dbObj.LoginName,
                LoginPassword = dbObj.LoginPassword,
                Salt = dbObj.Salt,
                LastLoginTime = IbexTimeStampToDateTimeOffset(siteTimeZone, dbObj.LastLoginTime,
                    logger),
                FailedLoginAttempts = dbObj.FailedLoginAttempts,
                UserSettings = MapUserSettings(dbObj.MedicationServicesAccess)
            };

            return ret;
        }

        private static IEnumerable<InboundUserSettingsDto> MapUserSettings(string medicationServicesAccess)
        {
            if (string.IsNullOrWhiteSpace(medicationServicesAccess))
                return null;

            // TODO: Show David
            return new List<InboundUserSettingsDto>
            {
                // This setting is pulled from the idex.dbo.drs table
                new InboundUserSettingsDto {SettingString = "MEDICATION_SERVICES", SettingValue = medicationServicesAccess},

                // These settings are hard-coded with default values David made up
                new InboundUserSettingsDto {SettingString = "PATIENT_NAME_DISPLAY", SettingValue = "Y"},
                new InboundUserSettingsDto {SettingString = "PATIENT_PAGE_SORT", SettingValue = "A"},
                new InboundUserSettingsDto {SettingString = "DEPARTMENT_PAGE_SORT", SettingValue = "A"},
                new InboundUserSettingsDto {SettingString = "DEPARTMENT_PAGE_FILTERING", SettingValue = "P"},
                new InboundUserSettingsDto {SettingString = "LAST_USED_PRINTER", SettingValue = "DEFAULT_NOT_DEFINED"}
            };
        }

        private static DateTimeOffset? IbexTimeStampToDateTimeOffset(string siteTimeZone, string ibexTimeStamp,
            ILogger<IIbexIdsProcessorService> logger)
        {
            if (string.IsNullOrEmpty(ibexTimeStamp) || ibexTimeStamp.Length < 3)
                return null;

            if (!int.TryParse(ibexTimeStamp, out var totalSeconds))
            {
                logger.LogWarning(
                    $"Found ibex.dbo.drs.last_login_time which didn't parse to an integer:  {ibexTimeStamp}");
                return null;
            }

            DateTimeOffset refDate = new DateTimeOffset(new DateTime(1970, 1, 1) + new TimeSpan(0, 0, totalSeconds), new TimeSpan(0, 0, 0, 0));

            var tzi = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z =>
                z.DisplayName == siteTimeZone || z.DaylightName == siteTimeZone || z.StandardName == siteTimeZone);

            if (tzi == null)
            {
                logger.LogWarning($"Invalid Timezone, \"{siteTimeZone}\" attached to sites record.");
                return null;
            }

            return TimeZoneInfo.ConvertTime(refDate, tzi);
        }

        public static InboundPatientDataDto MapPatient(EmarPatientsRetrieveView dbObj, ISiteRepository siteRepository,
            ILogger<IIbexIdsProcessorService> logger)
        {
            throw new NotImplementedException();
        }
    }
}


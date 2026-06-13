using Emar.Core.InboundData.Service.IbexSpecific;
using Emar.Core.Sites.Repository;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;
using Emar.Data.IbexEntities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Core.InboundData.Model.Mappings
{
    public static class IbexInboundMapper
    {
        public static User MapUser(EmarUsersRetrieveView dbObj, ISiteRepository siteRepository, IUserRepository repository,
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

            var ret = new User
            {
                ExternalId = dbObj.Id.ToString(),
                SiteId = siteId,
                Type = dbObj.Type,
                IsActive = dbObj.IsActive == 1,
                UserInitials = dbObj.InitialsDisplay,
                FirstName = dbObj.FirstName,
                LastName = dbObj.LastName,
                MiddleName = dbObj.MiddleName,
                NameSuffix = dbObj.NameSuffix,
                OrderingOnlyPhysician = dbObj.OrderingOnlyPhysician == 1,
                DisplayInitialsIndicator = dbObj.NameDisplayInitials == 1,
                LoginName = dbObj.LoginName,
                LoginPassword = dbObj.LoginPassword,
                Salt = dbObj.Salt,
                LastLoginTime = IbexTimeStampToDateTimeOffset(siteTimeZone, dbObj.LastLoginTime,
                    logger),
                FailedLoginAttempts = dbObj.FailedLoginAttempts,
                UserSettings = MapUserSettings(repository, dbObj.MedicationServicesAccess, siteId)
            };

            return ret;
        }

        private static List<UserSetting> MapUserSettings(IUserRepository repository,
            string medicationServicesAccess, int internalSiteId)
        {
            if (string.IsNullOrWhiteSpace(medicationServicesAccess))
                return null;

            return new List<UserSetting>
            {
                // This setting is pulled from the idex.dbo.drs table
                new UserSetting
                {
                    SettingId = repository.GetSettingId("MEDICATION_SERVICES"), SettingValue = medicationServicesAccess,
                    DefaultOnlySetting = false, SiteId = internalSiteId
                },

                // These settings are hard-coded with default values David made up
                // They are to be used as default values if the user doesn't have an existing value.
                // If it does exist, leave it as is.  (Accomplished with "DefaultOnlySetting=true"
                new UserSetting
                {
                    SettingId = repository.GetSettingId("PATIENT_NAME_DISPLAY"), SettingValue = "Y",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                },
                new UserSetting
                {
                    SettingId = repository.GetSettingId("PATIENT_PAGE_SORT"), SettingValue = "A",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                },
                new UserSetting
                {
                    SettingId = repository.GetSettingId("DEPARTMENT_PAGE_SORT"), SettingValue = "A",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                },
                new UserSetting
                {
                    SettingId = repository.GetSettingId("DEPARTMENT_PAGE_FILTERING"), SettingValue = "P",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                },
                new UserSetting
                {
                    SettingId = repository.GetSettingId("LAST_USED_PRINTER"), SettingValue = "DEFAULT_NOT_DEFINED",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                }
            };
        }

        public static Patient MapPatient(EmarPatientsRetrieveView dbObj, ISiteRepository siteRepository,
            ILogger<IIbexIdsProcessorService> logger)
        {
            if (dbObj == null)
                return null;

            // Need to convert:
            //  - external site to internal
            //  - vs_datetime to datetimeoffset for the subject timezone based on source.vsdate
            //  - IsActive = 1 for pat records, 0 for hst records
            int internalSiteId = siteRepository.GetInternalSiteId(dbObj.ExternalSiteId);
            var siteTimeZone = siteRepository.GetSiteTimeZone(internalSiteId);

            // Required fields in emar:
            // - id (IDENTITY)
            // - site_id - Required in ibex
            // - last_name - Required in ibex
            // - first_name - Required in ibex
            // - gender - Required in ibex
            // SQL View is providing default
            // - is_active
            // Providing default in this mapping (e.g. "dbObj.WithdrawConsent == 1" - if null, it will be false)
            // - name_alert - Default here to 0 (bit) (if 
            // - withdraw_consent - Default here to 0 (bit)

            var ret = new Patient()
            {
                ExternalPatientId = dbObj.ExternalId,
                ExternalSiteId = dbObj.ExternalSiteId,
                SiteId = internalSiteId,
                MedicalRecordNumber = dbObj.MedicalRecordNumber,
                AccountNumber = dbObj.AccountNumber,
                LastName = dbObj.LastName,
                FirstName = dbObj.FirstName,
                MiddleName = dbObj.MiddleName,
                NameSuffix = dbObj.NameSuffix,
                Gender = dbObj.Gender,
                DateOfBirth = dbObj.DateOfBirth,
                Age = byte.TryParse(dbObj.Age, out var age) ? age : (byte?)null,
                // AgeUnits is a char(1) in the DB, so assigning the empty string will give us ratcheting updates
                // Same for many of the following fields which are char(1)
                AgeUnits = dbObj.AgeUnits == "" ? " " : dbObj.AgeUnits,
                Complaint = dbObj.Complaint,
                HeightInCm = decimal.TryParse(dbObj.HeightInCm, out var height) ? height : (decimal?)null,
                WeightInKg = decimal.TryParse(dbObj.WeightInKg, out var weight) ? weight : (decimal?)null,
                RoomBedCode = dbObj.RoomBedCode,
                WardCode = dbObj.WardCode,
                DepartmentCode = dbObj.DepartmentCode,
                Urgency = dbObj.Urgency,
                UrgencyColor = dbObj.UrgencyColor,
                NameAlert = dbObj.NameAlert == 1,
                WithdrawConsent = dbObj.WithdrawConsent == 1,
                VsDatetime = IbexTimeStampToDateTimeOffset(siteTimeZone, dbObj.VsDatetime,
                    "ibex.dbo.emar_patients_retrieve_view.vs_datetime", logger),
                VsBloodPressureIndicator = dbObj.VsBloodPressureIndicator == "" ? " " : dbObj.VsBloodPressureIndicator,
                VsSystolic = dbObj.VsSystolic,
                VsDiastolic = dbObj.VsDiastolic,
                VsPulseIndicator = dbObj.VsPulseIndicator == ""?" ": dbObj.VsPulseIndicator,
                VsPulse = dbObj.VsPulse,
                VsMapLevel = dbObj.VsMapLevel == "" ? " " : dbObj.VsMapLevel,
                VsMap = dbObj.VsMap,
                VsRespiratoryIndicator = dbObj.VsRespiratoryIndicator == "" ? " " : dbObj.VsRespiratoryIndicator,
                VsRespiratory = dbObj.VsRespiratory,
                VsTemperatureIndicator = dbObj.VsTemperatureIndicator == "" ? " " : dbObj.VsTemperatureIndicator,
                VsTemperature = dbObj.VsTemperature,
                VsEndTidalLevel = dbObj.VsEndTidalLevel == "" ? " " : dbObj.VsEndTidalLevel,
                VsEndTidal = dbObj.VsEndTidal,
                VsOxygenSaturationIndicator = dbObj.VsOxygenSaturationIndicator == "" ? " " : dbObj.VsOxygenSaturationIndicator,
                VsOxygenSaturation = dbObj.VsOxygenSaturation,
                VsPainScaleIndicator = dbObj.VsPainScaleIndicator == "" ? " " : dbObj.VsPainScaleIndicator,
                VsPainScale = dbObj.VsPainScale,
                Active = dbObj.IsActive == 1,
                CustomNumber = dbObj.CustomNumber,
                PersonNumber = dbObj.PersonNumber,
                // David Mehegan said he would manage this with a SQL trigger
                //DeactivationDatetime = dbObj.de,
                VisitStartDatetime = IbexTimeStampToDateTimeOffset(siteTimeZone, dbObj.VisitStartDatetime,
                    "ibex.dbo.emar_patients_retrieve_view.visit_start_datetime", logger),
                GenderSystem = dbObj.GenderSystem,
                DispositionTypeCode = dbObj.DispositionTypeCode,
                DispositionCode = dbObj.DispositionCode
            };

            return ret;
        }

        public static EmarPatientsRetrieveView MapArchivedPatientToPatient(EmarArchivedPatientsRetrieveView dbObj)
        {
            if (dbObj == null)
                return null;

            return new EmarPatientsRetrieveView()
            {
                ExternalId = dbObj.ExternalId,
                ExternalSiteId = dbObj.ExternalSiteId ?? 0,
                MedicalRecordNumber = dbObj.MedicalRecordNumber,
                AccountNumber = dbObj.AccountNumber,
                LastName = dbObj.LastName,
                FirstName = dbObj.FirstName,
                MiddleName = dbObj.MiddleName,
                NameSuffix = dbObj.NameSuffix,
                Gender = dbObj.Gender,
                DateOfBirth = dbObj.DateOfBirth,
                Age = dbObj.Age,
                AgeUnits = dbObj.AgeUnits,
                Complaint = dbObj.Complaint,
                HeightInCm = dbObj.HeightInCm,
                WeightInKg = dbObj.WeightInKg,
                RoomBedCode = dbObj.RoomBedCode,
                WardCode = dbObj.WardCode,
                DepartmentCode = dbObj.DepartmentCode,
                Urgency = dbObj.Urgency,
                UrgencyColor = dbObj.UrgencyColor,
                NameAlert = dbObj.NameAlert,
                WithdrawConsent = dbObj.WithdrawConsent,
                VsDatetime = dbObj.VsDatetime,
                VsBloodPressureIndicator = dbObj.VsBloodPressureIndicator,
                VsSystolic = dbObj.VsSystolic,
                VsDiastolic = dbObj.VsDiastolic,
                VsPulseIndicator = dbObj.VsPulseIndicator,
                VsPulse = dbObj.VsPulse,
                VsMapLevel = dbObj.VsMapLevel,
                VsMap = dbObj.VsMap,
                VsRespiratoryIndicator = dbObj.VsRespiratoryIndicator,
                VsRespiratory = dbObj.VsRespiratory,
                VsTemperatureIndicator = dbObj.VsTemperatureIndicator,
                VsTemperature = dbObj.VsTemperature,
                VsEndTidalLevel = dbObj.VsEndTidalLevel,
                VsEndTidal = dbObj.VsEndTidal,
                VsOxygenSaturationIndicator = dbObj.VsOxygenSaturationIndicator,
                VsOxygenSaturation = dbObj.VsOxygenSaturation,
                VsPainScaleIndicator = dbObj.VsPainScaleIndicator,
                VsPainScale = dbObj.VsPainScale,
                CustomNumber = dbObj.CustomNumber,
                PersonNumber = dbObj.PersonNumber,
                VisitStartDatetime = dbObj.VisitStartDatetime,
                GenderSystem = dbObj.GenderSystem,
                IsActive = dbObj.IsActive,
                DispositionTypeCode = dbObj.DispositionTypeCode,
                DispositionCode = dbObj.DispositionCode
            };
        }

        #region Private Methods

        private static DateTimeOffset? IbexTimeStampToDateTimeOffset(string siteTimeZone, int? ibexTimeStamp,
        ILogger<IIbexIdsProcessorService> logger)
        {
            if (!ibexTimeStamp.HasValue)
                return null;

            DateTimeOffset refDate = new DateTimeOffset(new DateTime(1970, 1, 1)
                                                        + new TimeSpan(0, 0, ibexTimeStamp.Value),
                new TimeSpan(0, 0, 0, 0));

            var tzi = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z =>
                z.DisplayName == siteTimeZone || z.DaylightName == siteTimeZone || z.StandardName == siteTimeZone);

            if (tzi == null)
            {
                logger.LogWarning($"Invalid Timezone, \"{siteTimeZone}\" attached to sites record.");
                return null;
            }

            return TimeZoneInfo.ConvertTime(refDate, tzi);
        }

        internal static DateTimeOffset? IbexTimeStampToDateTimeOffset(string siteTimeZone, string ibexDateTime,
            string source, ILogger logger)
        {
            if (string.IsNullOrEmpty(ibexDateTime) || ibexDateTime.Length < 3)
                return null;

            // Normalize the string we're getting - sometimes it is just yyyymmdd. Sometimes yyyymmddhhmmssmmmmm.
            if (ibexDateTime.Length < 14)
                ibexDateTime += new string('0', 14 - ibexDateTime.Length);
            else if (ibexDateTime.Length > 14)
                ibexDateTime = ibexDateTime.Substring(0, 14);

            if (!long.TryParse(ibexDateTime, out var tempTimeInteger))
            {
                logger.LogWarning(
                    $"Found {source} which didn't parse to an integer:  {ibexDateTime}");
                return null;
            }

            var year = int.Parse(ibexDateTime.Substring(0, 4));
            var month = int.Parse(ibexDateTime.Substring(4, 2));
            var day = int.Parse(ibexDateTime.Substring(6, 2));
            var hour = int.Parse(ibexDateTime.Substring(8, 2));
            var minute = int.Parse(ibexDateTime.Substring(10, 2));
            var second = int.Parse(ibexDateTime.Substring(12, 2));

            var tzi = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z =>
                z.DisplayName == siteTimeZone || z.DaylightName == siteTimeZone || z.StandardName == siteTimeZone);

            if (tzi == null)
            {
                logger.LogWarning($"Invalid Timezone, \"{siteTimeZone}\" attached to sites record.");
                return null;
            }

            return new DateTimeOffset(new DateTime(year, month, day, hour, minute, second),
                tzi.GetUtcOffset(new DateTime(year, month, day)));
        }

        #endregion
    }
}


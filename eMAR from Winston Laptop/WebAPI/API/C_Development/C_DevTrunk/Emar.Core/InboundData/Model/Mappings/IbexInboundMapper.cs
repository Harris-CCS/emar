using Emar.Core.Devices.Repository;
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
        public static User MapUser(EmarUsersRetrieveView dbObj, IDeviceRepository deviceRepository, ISiteRepository siteRepository, IUserRepository repository,
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
                UserSettings = MapUserSettings(repository, dbObj.MedicationServicesAccess, siteId, dbObj.MedPrn, deviceRepository)
            };

            return ret;
        }

        private static List<UserSetting> MapUserSettings(IUserRepository repository,
            string medicationServicesAccess, int internalSiteId, int medPrn, IDeviceRepository deviceRepository)
        {
            if (string.IsNullOrWhiteSpace(medicationServicesAccess))
                return null;

            //Copy the last device used from PCED into emar.
            //This is drs.medprn in ibex.
            //It is null, -1, 0, or an actual id in the DB.
            //So I added a case to the view to convert null and -1 to 0.
            //If it's 0, then it's not a device id and we'll use the default value.
            //If it's not 0, then it is a device ia, and we'll use that.
            //Once we've got the ibex device id, we'll need to use that to get the emar device id from the external_ids table.
            //Winston Murdock, 07/14/2021.  EMAR-1087.
            string lastUsedDeviceId = "";
            if (medPrn == 0)
            {
                lastUsedDeviceId = "DEFAULT_NOT_DEFINED";
            }
            else
            {
                //Convert the medprn value (device id) to a string.
                lastUsedDeviceId = medPrn.ToString();

                //Use the emar site id to get the ibex site id.
                var extSiteId = Convert.ToInt32(deviceRepository.GetExtSiteId(internalSiteId));

                //Use the external site id and the ibex device id to get the internal device id.
                lastUsedDeviceId = deviceRepository.GetInternalDeviceId(extSiteId, lastUsedDeviceId);
            } //end if

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
                    //We messed up the the settings between sort and filter on mar department.
                    //We're going to run a SQL script to fix the data.  And we're making this change so that
                    //the initial defaults are set correctly for new users.
                    //Winston Murdock, 06/14/2021.  EMAR-951. 
                    //SettingId = repository.GetSettingId("DEPARTMENT_PAGE_SORT"), SettingValue = "A",
                    SettingId = repository.GetSettingId("DEPARTMENT_PAGE_SORT"), SettingValue = "P",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                },
                new UserSetting
                {
                    //We messed up the the settings between sort and filter on mar department.
                    //We're going to run a SQL script to fix the data.  And we're making this change so that
                    //the initial defaults are set correctly for new users.
                    //Winston Murdock, 06/14/2021.  EMAR-951. 
                    //SettingId = repository.GetSettingId("DEPARTMENT_PAGE_FILTERING"), SettingValue = "P",
                    SettingId = repository.GetSettingId("DEPARTMENT_PAGE_FILTERING"), SettingValue = "A",
                    DefaultOnlySetting = true, SiteId = internalSiteId
                },
                new UserSetting
                {
                    //Use the last used device id calculated above rather than hardcoding "DEFAULT_NOT_DEFINED".
                    //I also changed DefaultOnlySetting to false since this is something we pull in from the IDS.
                    //When that is set to true, then we don't update this one.
                    //That's logical for things like mar department filter which don't exist in PCED
                    //and which are set to logical defaults for a new user.  But we do want to update this from PCED.
                    //Winston Murdock, 07/14/2021.  EMAR-1087.
                    SettingId = repository.GetSettingId("LAST_USED_PRINTER"), SettingValue = lastUsedDeviceId,
                    DefaultOnlySetting = false, SiteId = internalSiteId
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
                    "ibex.dbo.emar_patients_retrieve_view.vs_datetime", logger, siteRepository),
                VsBloodPressureIndicator = dbObj.VsBloodPressureIndicator == "" ? " " : dbObj.VsBloodPressureIndicator,
                VsSystolic = dbObj.VsSystolic,
                VsDiastolic = dbObj.VsDiastolic,
                VsPulseIndicator = dbObj.VsPulseIndicator == "" ? " " : dbObj.VsPulseIndicator,
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
                Active = dbObj.IsActive == 1 && dbObj.EmarPat == "Y",
                CustomNumber = dbObj.CustomNumber,
                PersonNumber = dbObj.PersonNumber,
                // David Mehegan said he would manage this with a SQL trigger
                //DeactivationDatetime = dbObj.de,
                VisitStartDatetime = IbexTimeStampToDateTimeOffset(siteTimeZone, dbObj.VisitStartDatetime,
                    "ibex.dbo.emar_patients_retrieve_view.visit_start_datetime", logger, siteRepository),
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
                IsActive = 0,
                DispositionTypeCode = dbObj.DispositionTypeCode,
                DispositionCode = dbObj.DispositionCode,
                EmarPat = dbObj.EmarPat
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
            string source, ILogger logger, ISiteRepository? siteRepository = null)
        {
            if (string.IsNullOrEmpty(ibexDateTime) || ibexDateTime.Length < 3)
                return null;

            DateTime? interpretedDate = null;
            DateTime tempDateTime;


            //This date comes in as a 14-digit number (similar to how HL7 formats dates).
            //YYYYMMDDhhmmss is the format.
            //Switched from using string operations (substring 1-4, substring 5-6, etc...) to
            //using math operations since the math operations are faster.
            //The places where we do x % y / z, "%" and "/" are equal in precedence for order of operations.
            //x % y /x is logically the same as (x % y) / z.
            if (long.TryParse(ibexDateTime, out var tempTimeInteger))
            {
                if (ibexDateTime.Length < 14)
                {
                    ibexDateTime += new string('0', 14 - ibexDateTime.Length);
                    tempTimeInteger = long.Parse(ibexDateTime);
                }

                //1) Get digits 1-4 as the year.
                //2) Get digits 5-6 as the month.
                //3) Get digits 7-8 as the day.
                //4) Get digits 9-10 as the hour.
                //5) Get digits 11-12 as the minutes.
                //6) Get digits 13-14 as the seconds.

                //code changes Brad Marshall, 06/23/2022.
                //Comments Winston Murdock, 06/24/2022.
                var year = (int)(tempTimeInteger / 10000000000);
                var month = (int)(tempTimeInteger % 10000000000 / 100000000);
                var day = (int)(tempTimeInteger % 100000000 / 1000000);
                var hour = (int)(tempTimeInteger % 1000000 / 10000);
                var minute = (int)(tempTimeInteger % 10000 / 100);
                var second = (int)(tempTimeInteger % 100);
                try
                {
                    interpretedDate = new DateTime(year, month, day, hour, minute, second);
                }
                catch (Exception)
                {
                    logger.LogWarning(
                        $"Found {source} which didn't parse to an integer:  {ibexDateTime}");
                    return null;
                }
            }
            else if (DateTime.TryParse(ibexDateTime, out tempDateTime))
            {
                interpretedDate = tempDateTime;
            }
            else
            {
                // Saw some values from ibex in the forms:  "6/29/2017 03" and "11/20/2017 1", "1/5/2016 06:"
                var parts = ibexDateTime.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var temp))
                {
                    switch (parts[1].Length)
                    {
                        case 1:
                            // Real example from 57c - "11/20/2017 1"
                            // 0, 1, or 2 -- but we can't really tell even the hour from its first digit
                            interpretedDate = temp;
                            break;
                        case 2:
                            // Real example from 57c - "6/29/2017 03"
                            // If it parses to an int, and it is betwen 0 and 23, it's an hour, so just slap :00 on the end
                            if (int.TryParse(parts[1], out var hours))
                            {
                                if (hours >= 0 && hours <= 23)
                                {
                                    ibexDateTime += ":00";
                                    if (DateTime.TryParse(ibexDateTime, out tempDateTime))
                                        interpretedDate = tempDateTime;
                                }
                            }
                            if (!interpretedDate.HasValue)
                                interpretedDate = temp;
                            break;
                        case 3:
                        case 4:
                            // Real example from 57c for case 3 - "1/5/2016 06:"
                            // Don't think there is going to be a case for 4, since both month and day being single-digit
                            // results in 3, and you can't go less than single-digit...  Regardless, since the minutes
                            // would be ambiguous for 4, treat it the same way as 3 and just forget about the minutes.
                            if (int.TryParse(parts[1].Substring(0, 2), out var hours2) && parts[1].Substring(2, 1) == ":")
                            {
                                if (hours2 >= 0 && hours2 <= 23)
                                {
                                    ibexDateTime += "00";
                                    if (DateTime.TryParse(ibexDateTime, out tempDateTime))
                                        interpretedDate = tempDateTime;
                                }
                            }
                            if (!interpretedDate.HasValue)
                                interpretedDate = temp;

                            break;
                        default:
                            interpretedDate = temp;
                            break;
                    }
                }
            }
            if (!interpretedDate.HasValue)
                return null;

            //Get the site's time zone (central, mountain, etc...).
            //This tell us which time zone, but not the offset yet.
            //Below, we'll pass in the patient's arrival date to get the offset at that time.
            //If that date is within daylight savings time, then central time will have an offset of -5.
            //If that date is not within daylight savigns time, then central time will have an offset of -6.
            //This gets us past the March 13 issue of being one hour off without requiring us to go pull the offset from the DB.
            //And it also gives an added benefit of protecting us from patient updates on the switchover dates.
            //Say we had a patient come in at 1:59 AM on daylight savings time switch off day, then IDS
            //pulls them in at the second 1:01 AM time.  This way we'll still use that 1:59 AM timestamp and not mess
            //up their arrival date/time by up to an hour.
            //This function is called for other date values too (vital signs and others).
            //But arrival time was what first put it on my radar back in March.
            //This has been thoroughly tested.  We can run it with mulitple date/time values and always get the correct offset.
            //Just before daylight savings time switch on in March gives us an offset of -6 for central time.
            //Just after daylight savings time switch on in March gives us an offset of -5 for central time.
            //Code change Brad Marshall, 06/23/2022.
            //Comment Winston Murdock, 06/24/2022.
            TimeZoneInfo tzi = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(z =>
                z.DisplayName == siteTimeZone || z.DaylightName == siteTimeZone || z.StandardName == siteTimeZone);

            if (tzi != null)
            {
                return new DateTimeOffset(interpretedDate.Value, tzi.GetUtcOffset(interpretedDate.Value));
            }

            logger.LogWarning($"Invalid Timezone, \"{siteTimeZone}\" attached to sites record.");
            throw new ArgumentException($"Value {siteTimeZone} could not be mapped to a TimeZoneInfo", nameof(siteTimeZone));
        }


        #endregion
    }
}


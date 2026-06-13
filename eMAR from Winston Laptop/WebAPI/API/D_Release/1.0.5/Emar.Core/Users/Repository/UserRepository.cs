using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Emar.Core.Users.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly EmarContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly MemoryCache _cache;


        public UserRepository(EmarContext emarContext, ILogger<UserRepository> logger, EmarMemoryCache cache)
        {
            _context = emarContext;
            _logger = logger;
            _cache = cache.Cache;
        }

        public IEnumerable<User> GetUsers()
        {
            var users = _context.Users;

            return users;
        }

        public User GetUser(int userId)
        {
            return GetUsers(u => u.Id == userId)
                    .FirstOrDefault();
        }

        public int GetInternalUserId(string extId)
        {
            // If no external ID is found, 0 will be returned.
            return _cache.GetOrCreate(extId + CacheKeys.UserExternalIdKeys, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                var ret = (int)
                    (from e in _context.ExternalIds
                        where e.ExternalId == extId &&
                              e.Entity == "users" &&
                              e.Vendor == "pulsecheck"
                        select e.InternalId).FirstOrDefault();
                entry.Size = 1;
                return ret;
            });
        }

        public User GetUser(string loginName)
        {
            return GetUsers(u => u.LoginName == loginName)
                    .FirstOrDefault();
        }

        public User GetUserByExternalId(string extId)
        {
            var userId = GetInternalUserId(extId);

            return userId <= 0 ? null : GetUsers(u => u.Id == userId)
                .FirstOrDefault();
        }

        public IEnumerable<User> GetOrderingPhysicians(int siteId)
        {
            return GetUsersWithoutSite(u => u.SiteId == siteId && u.IsActive && (u.Type == "D" || (u.OrderingOnlyPhysician ?? false)));
        }

        private IEnumerable<User> GetUsersWithoutSite(Expression<Func<User, bool>> wherePredicate)
        {
            return _context.Users
                .Where(wherePredicate).ToList();
        }

        private IEnumerable<User> GetUsers(Expression<Func<User, bool>> wherePredicate)
        {
            return _context.Users
                    .Include(u => u.Site)
                    .Include(u => u.UserSettings)
                    .Where(wherePredicate).ToList();
        }

        public string GetUserDefaultMarFilters(int userId)
        {
            //SELECT setting_value
            //FROM user_settings
            //WHERE user_id = xxx
            //AND setting_id = 5
            string sRet = _context.UserSettings.Where(a => a.UserId == userId && a.SettingId == GetSettingId("DEPARTMENT_PAGE_FILTERING")).FirstOrDefault().SettingValue;

            //If we can't find a setting for this user, then use a logical default.
            if (sRet.Length < 1)
            {
                sRet = "A";
            } //end if

            //Return
            return sRet;
        } //end GetUserDefaultMarFilters


        public string SetUserDefaultFilters(int userId, PatientsResourceParameters resourceParameters)
        {
            //String to hold the new mar depratment filter settings for this user.
            //There are 8 possible values for the default filter.I’ve itemized them below.
            //A = all patients
            //AV = all patients with orders that need pharmacy verification
            //AU = all patients with upcoming orders
            //AVU = all patients with orders that need pharmacy verification and with upcoming orders
            //M = my patients
            //MV = my patients with orders that need pharmacy verification
            //MU = my patients with upcoming orders
            //MVU = my patients with orders that need pharmacy verification and with upcoming orders
            //Winston Murdock, 06/21/2021.  EMA-951.
            string sNewUserDefaultFilter = "";

            //Include my patients only.
            if (resourceParameters.IncludeMyPatientsOnly)
            {
                sNewUserDefaultFilter = "M";
            }
            else
            {
                sNewUserDefaultFilter = "A";
            } //end if

            //Pharmacy Verification Status
            if (resourceParameters.PharmacyVerificationStatus != null)
            {
                sNewUserDefaultFilter += "V";
            } //end if

            //Upcoming Orders Only
            if (resourceParameters.UpcomingOrdersOnly)
            {
                sNewUserDefaultFilter += "U";
            } //end if

            //Save the new defailt filter value to the DB for this user.
            UserSetting userSettingMarDepartmentFilter = _context.UserSettings.Where(a => a.UserId == resourceParameters.UserId && a.SettingId == GetSettingId("DEPARTMENT_PAGE_FILTERING")).FirstOrDefault();
            userSettingMarDepartmentFilter.SettingValue = sNewUserDefaultFilter;
            _context.SaveChanges();

            return sNewUserDefaultFilter;
        } //end SetUserDefaultFilters

        #region IDS Methods

        public void FileUser(User userFromHost, string externalUserId)
        {
            if (userFromHost.Id > 0)
            {
                User existingUser = _context.Users
                    .Include(u => u.UserSettings)
                    .FirstOrDefault(u => u.Id == userFromHost.Id);

                if (existingUser == null)
                {
                    _logger.LogWarning($"Made it into FileUser() with a userFromHost.Id of {userFromHost.Id}" +
                                       $" which couldn't be retrieved from the database");
                    return;
                }

                ////  Nice concept, but dangerous.  If the only change is in a child record, then it won't be caught
                //var trackedEntry = _context.ChangeTracker.Entries<User>().First(u => u.Entity.Id == userFromHost.Id);
                //if (trackedEntry.State == EntityState.Unchanged)
                //    return;

                _context.Entry(existingUser).CurrentValues.SetValues(userFromHost);
                UpdateUserSettingsValues(existingUser, userFromHost);

                _context.SaveChanges();
                return;
            }

            // Save this new user to the database
            var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Add(userFromHost);
                _context.SaveChanges();

                //If this user already exists in the external_ids table, we don't
                //want to add them again. We are seeing an error where we try to
                //add a user into external_ids when they already exist in there.
                //I'm not sure this is where the issue is, but it's the most logical spot.
                //If there is already an ExternalIdEntity record where we match on
                //Entity, Vendor, and ExternalId, then do not attempt the add.
                //If there is not one, then do attempt the add.
                //This is faster than digging deeper to figure out why we're attempting
                //to add a user into the external_ids table that already exists in there.
                //Winston Murdock.  12/14/2021. PC-26697
                if (!_context.ExternalIds.Any(x => x.Entity == "users" && x.Vendor == "pulsecheck" && x.ExternalId == externalUserId))
                {
                    //This PCED user does not exist in the external_ids table.
                    //Try to add them.
                    var externalId = new ExternalIdEntity
                    {
                        Entity = "users",
                        Vendor = "pulsecheck",
                        InternalId = userFromHost.Id,
                        ExternalId = externalUserId
                    };

                    _context.Add(externalId);
                    _context.SaveChanges();
                    transaction.Commit();
                } //end if (does this PCED user already exist in the external_ids table?)
            }
            catch (Exception e)
            {
                transaction.Rollback();
                _logger.LogError(
                    "Exception Encountered when filing new user from UserRepository.FileUser(): " +
                    $"{Utilities.ExtractExceptionMessages(e)}");
            }
        }

        public void DeactivateUser(int userId)
        {
            User user = _context.Users.FirstOrDefault(u => u.Id == userId);

            // Should never be NULL (we checked on the way in) but better safe than...
            if (user == null)
                return;

            user.IsActive = false;
            _context.SaveChanges();
        }

        private static void UpdateUserSettingsValues(User existingUser, User userFromHost)
        {
            // Pull in the current settings
            //  NOTE:  Only SettingId, SettingValue and SiteId are populated from the Ibex data
            foreach (var setting in userFromHost.UserSettings)
            {
                var existingSetting =
                    existingUser.UserSettings
                        //  NOTE:  Primary key on user_settings is userid, siteid, settingid
                        // (don't need to worry about userid, since it was part of the .Include()
                        .FirstOrDefault(s => s.SiteId == setting.SiteId && s.SettingId == setting.SettingId);

                if (existingSetting == null)
                    existingUser.UserSettings.Add(setting);
                // If the setting we got from the host system is tagged as "DefaultOnlySetting"
                // then don't use it if the user already has a value for that setting
                //Better safe than sorry, skip this if it's a null value.
                //Winston Murdock, 01/28/2022.  PC-26949
                else if ((!setting.DefaultOnlySetting) && (!string.IsNullOrWhiteSpace(setting.SettingValue)))
                    existingSetting.SettingValue = setting.SettingValue;
            }
        }

        public int GetSettingId(string settingString)
        {
            var settingsDict = GetSettingsDictionary();

            if (settingsDict.ContainsValue(settingString))
                return settingsDict.FirstOrDefault(kvp => kvp.Value == settingString).Key;

            _logger.LogError($"[settings] table doesn't have a record that has a [name] of {settingString}");
            return -1;
        }

        public string GetSettingString(int settingId)
        {
            var settingsDict = GetSettingsDictionary();

            if (settingsDict.TryGetValue(settingId, out var settingString))
                return settingString;

            _logger.LogError($"[settings] table doesn't have a record that has an [id] of {settingId}");
            return "";
        }

        private Dictionary<int, string> GetSettingsDictionary()
        {
            return _cache.GetOrCreate("All" + CacheKeys.UserSettingKeys, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var ret =
                    _context.Settings
                        .ToDictionary(setting => setting.Id, setting => setting.Name);
                entry.Size = ret.Count;
                return ret;
            });
        }
    }

    #endregion
}
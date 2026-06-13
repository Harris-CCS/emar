using Emar.Core.Helpers;
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
        private MemoryCache _cache;


        public UserRepository()
        {

        }

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
            var userId = from e in _context.ExternalIds
                         where e.ExternalId == extId &&
                               e.Entity == "users" &&
                               e.Vendor == "pulsecheck"
                         select e.InternalId;

            return (int)userId.FirstOrDefault();
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

        #region IDS Methods

        public void FileUser(User userFromHost, string externalUserId)
        {
            if (userFromHost.Id > 0)
            {
                User existingUser = _context.Users
                    .Include(u => u.UserSettings)
                    .FirstOrDefault(u => u.Id == userFromHost.Id);
                UpdateUserValues(existingUser, userFromHost);
                ////  Nice concept, but dangerous.  If the only change is in a child record, then it won't be caught
                //var trackedEntry = _context.ChangeTracker.Entries<User>().First(u => u.Entity.Id == userFromHost.Id);
                //if (trackedEntry.State == EntityState.Unchanged)
                //    return;
                _context.SaveChanges();
                return;
            }

            // Save this new user to the database
            // TODO: Put the below code in a transaction so that we don't get the user without the external_ids record
            try
            {
                _context.Add(userFromHost);
                _context.SaveChanges();

                var externalId = new ExternalIdEntity
                {
                    Entity = "users",
                    Vendor = "pulsecheck",
                    InternalId = userFromHost.Id,
                    ExternalId = externalUserId
                };

                _context.Add(externalId);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
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

        private static void UpdateUserValues(User existingUser, User userFromHost)
        {
            existingUser.SiteId = userFromHost.SiteId;
            existingUser.Type = userFromHost.Type;
            existingUser.IsActive = userFromHost.IsActive;
            existingUser.UserInitials = userFromHost.UserInitials;
            existingUser.FirstName = userFromHost.FirstName;
            existingUser.LastName = userFromHost.LastName;
            existingUser.MiddleName = userFromHost.MiddleName;
            existingUser.NameSuffix = userFromHost.NameSuffix;
            existingUser.OrderingOnlyPhysician = userFromHost.OrderingOnlyPhysician;
            existingUser.DisplayInitialsIndicator = userFromHost.DisplayInitialsIndicator;
            existingUser.LoginName = userFromHost.LoginName;
            existingUser.LoginPassword = userFromHost.LoginPassword;
            // Bypassing Salt for now.  Kinda weird handling of byte[] stuff, and we actually don't care use it
            //existingUser.Salt = userFromHost.Salt;
            existingUser.LastLoginTime = userFromHost.LastLoginTime;
            existingUser.FailedLoginAttempts = userFromHost.FailedLoginAttempts;

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
                else
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
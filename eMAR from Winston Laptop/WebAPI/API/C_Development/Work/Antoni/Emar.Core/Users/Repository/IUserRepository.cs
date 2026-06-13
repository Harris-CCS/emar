using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Users.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(int userId);
        int GetInternalUserId(string extId);
        User GetUser(string loginName);
        User GetUserByExternalId(string extId);
        IEnumerable<User> GetOrderingPhysicians(int siteId);

        // IDS Methods
        void FileUser(User userFromHost, string externalUserId);
        int GetSettingId(string settingString);
        string GetSettingString(int settingId);
        void DeactivateUser(int userId);
    }
}
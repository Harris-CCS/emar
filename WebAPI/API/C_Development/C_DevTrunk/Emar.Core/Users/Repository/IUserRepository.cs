using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Users.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();
        User GetUser(int userId);
        long? GetInternalUserId(string extId);
    }
}
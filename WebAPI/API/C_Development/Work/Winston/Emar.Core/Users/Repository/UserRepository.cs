using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.Users.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly EmarContext _context;

        public UserRepository()
        {

        }

        public UserRepository(EmarContext emarContext)
        {
            _context = emarContext;
        }

        public IEnumerable<User> GetUsers()
        {
            var users = _context.Users;

            return users;
        }

        public User GetUser(int userId)
        {
            var user = _context.Users
                        .Include(u => u.Site)
                        .FirstOrDefault(u => u.Id == userId);
            //.FirstOrDefault(u => u.Site.Id == u.SiteId);

            return user;
        }

        public long? GetInternalUserId(string extId)
        {
            var userId = from e in _context.ExternalIds
                         where e.ExternalId == extId &&
                               e.Entity == "users" &&
                               e.Vendor == "pulsecheck"
                         select e.InternalId;

            return userId.FirstOrDefault();
        }

        public User GetUser(string loginName)
        {
            return _context.Users
                .Include(u => u.Site)
                .FirstOrDefault(u => u.LoginName == loginName);
        }
    }
}
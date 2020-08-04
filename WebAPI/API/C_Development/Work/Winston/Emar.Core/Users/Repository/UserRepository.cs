using System;
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
            return GetUsers(u => u.Id == userId)
                    .FirstOrDefault();
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
            return GetUsers(u => u.LoginName == loginName)
                    .FirstOrDefault();
        }

        public IEnumerable<User> GetUsers(Func<User, bool> wherePredicate)
        {
            return _context.Users
                    .Include(u => u.Site)
                    .Where(wherePredicate);
        }
    }
}
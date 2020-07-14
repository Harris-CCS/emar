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
            User user = _context.Users
                        .Where(u => u.Id == userId)
                        .Include(u => u.Site)
                        .FirstOrDefault(u => u.Site.Id == u.SiteId);

            return user;
        }

        public long? GetInternalUserId(int extId)
        {
            long userId = _context.ExternalIds
                            .Where(@x_id =>
                                    @x_id.External_Id.Equals(extId.ToString()) &&
                                    @x_id.Entity.ToLower().Equals(@"users") &&
                                    @x_id.Vendor.ToLower().Equals(@"pulsecheck"))
                            .FirstOrDefault()
                            .InternalId;
            return userId;
        }
    }
}
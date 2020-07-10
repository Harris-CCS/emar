using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Emar.Core.Users.Model;
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
            var user = _context.Users.Find(userId);

            return user;
        }
    }
}
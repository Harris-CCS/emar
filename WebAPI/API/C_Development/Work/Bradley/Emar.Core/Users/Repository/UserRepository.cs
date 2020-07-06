using System.Collections.Generic;
using Emar.Data;
using Emar.Data.Entities;

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
            IEnumerable<User> users = _context.Users;

            return users;
        }

        public User GetUser(int userId)
        {
            User user = _context.Users.Find(userId);

            return user;
        }
    }
}
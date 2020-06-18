using Emar.Data;

namespace Emar.Core.Users.Repository
{
    public class UserRepository:IUserRepository
    {
        public User GetUser(in int userId)
        {

            var user = new User {Id = userId, Title = "Lord Chief", Name = "PTurnbull"};
            return user;
        }
    }
}
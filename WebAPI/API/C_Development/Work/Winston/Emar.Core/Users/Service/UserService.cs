using Emar.Core.Users.Model;
using Emar.Core.Users.Model.Mappings;
using Emar.Core.Users.Repository;

namespace Emar.Core.Users.Service
{
    public class UserService : IUserService
    {
        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public UserDto GetUser(in int userId)
        {
            var entityUser = _userRepository.GetUser(userId);
            return UserMapper.MapUser(entityUser);
        }
    }
}
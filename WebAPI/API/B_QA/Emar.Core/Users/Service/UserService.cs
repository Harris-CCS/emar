using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<UserDto> GetUsers()
        {
            var users = _userRepository.GetUsers().ToList();

            var usersDto = users.Select(user => UserMapper.MapUser(user));

            return usersDto;
        }

        public UserDto GetUser(int userId)
        {
            var user = _userRepository.GetUser(userId);

            if (user == null)
            {
                return null;
            }

            var userDto = UserMapper.MapUser(user);

            return userDto;
        }
    }
}
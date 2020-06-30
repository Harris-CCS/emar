using System.Collections.Generic;
using Emar.Core.Users.Model;
using Emar.Core.Users.Model.Mappings;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;

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
            IEnumerable<User> entityUsers = _userRepository.GetUsers();
            List<UserDto> userList = new List<UserDto>();

            foreach (User user in entityUsers)
            {
                userList.Add(UserMapper.MapUser(user));
            }

            return userList;
        }

        public UserDto GetUser(in int userId)
        {
            User entityUser = _userRepository.GetUser(userId);
            UserDto userDto = UserMapper.MapUser(entityUser);

            return userDto;
        }
    }
}
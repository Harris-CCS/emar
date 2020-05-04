using Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainModel;
using Interfaces.Repository;
using System.Web;
using PulseCheck.Constants;
using System.Data.SqlClient;
using PulseCheck.Utilities;
using System.Data;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// UserService constructor
        /// </summary>
        /// <param name="userRepository">IUserRepository instance</param>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get a list of users by a list of IDs
        /// </summary>
        /// <param name="userIds">List of (int) user identifiers</param>
        /// <returns>List of User objects found</returns>
        public async Task<List<User>> GetUsersByIdAsync(List<int> userIds)
        {
            return await _userRepository.GetUsersByIdAsync(userIds);
        }

        /// <summary>
        /// Get a single user by ID
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User object found</returns>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        /// <summary>
        /// Get the user's list of favorite orders
        /// </summary>
        /// <param name="user">User object</param>
        /// <returns>List of Groups</returns>
        public async Task<List<DomainModel.Service>> GetUserFavoriteOrders(User user)
        {
            return await _userRepository.GetUserFavoriteOrders(user);
        }

        /// <summary>
        /// Add an order to the user's list of favorite orders
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="num">Order identifier</param>
        /// <returns>Int for result</returns>
        public int AddUserFavoriteOrder(User user, int num)
        {
            var result = _userRepository.AddUserFavoriteOrder(user, num);
            return result;
        }

        /// <summary>
        /// Remove an order from the user's list of favorite orders
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="num">Order identifier</param>
        /// <returns>Int for result</returns>
        public int RemoveUserFavoriteOrder(User user, int num)
        {
            return _userRepository.RemoveUserFavoriteOrder(user, num);
        }
    }
}

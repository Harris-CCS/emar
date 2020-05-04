using DomainModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    public interface IAdminService
    {
        Task<List<User>> GetUsersByIdAsync(List<int> userIds);
        Task<User> GetUserByIdAsync(int userId);
        Task<List<Group>> GetUserFavoriteOrders(User user);
        Task<int> AddUserFavoriteOrder(User user, int num);
        int RemoveUserFavoriteOrder(User user, int num);
    }
}

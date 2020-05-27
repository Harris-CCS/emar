using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.Domain;

namespace PulseCheck.ILogic
{
    public interface IUserManager
    {
        Task<List<User>> GetUsersByIdAsync(List<int> userIds);
        Task<User> GetUserByIdAsync(int userId);
        Task<List<Service>> GetUserFavoriteOrders(User user);
        int AddUserFavoriteOrder(User user, int num);
        int RemoveUserFavoriteOrder(User user, int num);
    }
}

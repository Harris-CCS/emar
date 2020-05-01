using DomainModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsersByIdAsync(List<int> userIds);
        Task<User> GetUserByIdAsync(int id);
        Task<List<DomainModel.Service>> GetUserFavoriteOrders(User user);
        int AddUserFavoriteOrder(User user, int num);
        int RemoveUserFavoriteOrder(User user, int num);
        Task<List<User>> SearchUsersForAccount(string login, string name);
    }
}

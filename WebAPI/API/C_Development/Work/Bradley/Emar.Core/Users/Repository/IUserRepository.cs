using Emar.Data.Entities;

namespace Emar.Core.Users.Repository
{
    public interface IUserRepository
    {
        User GetUser(in int userId);
    }
}
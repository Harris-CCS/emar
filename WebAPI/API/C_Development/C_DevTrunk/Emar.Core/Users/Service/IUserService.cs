using Emar.Core.Users.Model;

namespace Emar.Core.Users.Service
{
    public interface IUserService
    {
        UserDto GetUser(in int userId);
    }
}
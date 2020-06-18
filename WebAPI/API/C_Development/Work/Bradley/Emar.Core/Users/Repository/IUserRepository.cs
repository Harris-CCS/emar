namespace Emar.Core.Users.Repository
{
    public interface IUserRepository
    {
        Emar.Data.User GetUser(in int userId);
    }
}
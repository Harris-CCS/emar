using Emar.Data;

namespace Emar.Core.Users.Model.Mappings
{
    public static class UserMapper
    {
        public static UserDto MapUser(User entityUser)
        {
            var ret = new UserDto {Id = entityUser.Id, Name = entityUser.Name, Title = entityUser.Title};
            return ret;
        }
    }
}
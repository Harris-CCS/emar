using DomainModel.Membership;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    public interface IUserAccountService
    {
        UserAccount CreateUserAccount(UserAccountConfiguration newUser, string tempPassword);

        UserAccount EditUserAccount(UserAccount editedUser);

        void RemoveUserAccount(UserAccount removedUser);

        List<UserAccount> GetUserAccounts();

        string UpdatePassword(UserAccount account, string password);
    }
}

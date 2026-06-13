using System.Collections.Generic;
using PulseCheck.Domain.Membership;

namespace PulseCheck.ILogic
{
    public interface IUserAccountManager
    {
        UserAccount CreateUserAccount(UserAccountConfiguration newUser, string tempPassword);

        UserAccount EditUserAccount(UserAccount editedUser);

        void RemoveUserAccount(UserAccount removedUser);

        List<UserAccount> GetUserAccounts();

        string UpdatePassword(UserAccount account, string password);
    }
}

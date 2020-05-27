using System;
using System.Collections.Generic;
using System.Text;
using BrockAllen.MembershipReboot;
using PulseCheck.Constants;
using PulseCheck.Data.Repositories;
using PulseCheck.Domain.Membership;
using PulseCheck.ILogic;
using PulseCheck.Utilities;
using UserAccount = PulseCheck.Domain.Membership.UserAccount;

namespace PulseCheck.Logic
{
    public class UserAccountManager : UserAccountService<UserAccount>, IUserAccountManager
    {
        private UserAccountRepository _userAccountRepo;

        static MembershipRebootConfiguration<UserAccount> config;
        static UserAccountService<UserAccount> _svc;

        public UserAccountManager(UserAccountRepository repo) : base(repo)
        {
            _userAccountRepo = repo;

            config = new MembershipRebootConfiguration<UserAccount> {
                RequireAccountVerification = false,
                RequireAccountApproval = false,
                EmailIsUsername = true,
                EmailIsUnique = true,
            };

            _svc = new UserAccountService<UserAccount>(config, repo);
        }

        public UserAccount CreateUserAccount(UserAccountConfiguration newUser, string tempPassword)
        {
            var createdUser = _svc.CreateAccount(newUser.Email, tempPassword, newUser.Email);
            var finalUser = ChangeUserAccount(createdUser, newUser.Account);
            _svc.SetConfirmedEmail(createdUser.ID, newUser.Email);

            return finalUser;
        }

        public UserAccount EditUserAccount(UserAccount editedUser)
        {
            var existingUser = _svc.GetByID(editedUser.ID);
            return ChangeUserAccount(existingUser, editedUser);
        }

        public void RemoveUserAccount(UserAccount user)
        {
            _svc.DeleteAccount(user.ID);
        }

        

        public UserAccount ChangeUserAccount(UserAccount existing, UserAccount updated)
        {
            existing.LastName = updated.LastName;
            existing.FirstName = updated.FirstName;

            _svc.Update(existing);
            return existing;
        }

        public List<UserAccount> GetUserAccounts()
        {
            var userList = _userAccountRepo.GetAllAccounts();
            return userList;
        }

        /// <summary>
        /// Update an account's password
        /// </summary>
        /// <param name="account">UserAccount whose password is changing</param>
        /// <param name="password">New password</param>
        /// <return>Error message if password doesn't meet requirements, null otherwise</return>
        public string UpdatePassword(UserAccount account, string password)
        {
            var errorMessage = new StringBuilder();
            var minLength = Convert.ToInt32(Settings.GetSetting(Settings.Constants.PASSWORD_MINIMUM_LENGTH));
            var minSpecial = Convert.ToInt32(Settings.GetSetting(Settings.Constants.PASSWORD_MINIMUM_SPECIAL_CHARS));
            var minCaps = Convert.ToInt32(Settings.GetSetting(Settings.Constants.PASSWORD_MINIMUM_CAPS));
            var minNum = Convert.ToInt32(Settings.GetSetting(Settings.Constants.PASSWORD_MINIMUM_NUMBERS));

            var totalCaps = 0;
            var totalNum = 0;
            var totalSpecial = 0;
            foreach (char character in password)
            {
                if (char.IsUpper(character))
                {
                    totalCaps++;
                }
                else if (char.IsNumber(character))
                {
                    totalNum++;
                }
                else if (!char.IsLetterOrDigit(character))
                {
                    totalSpecial++;
                }
            }

            if (password.Length < minLength)
                errorMessage.AppendLine("Password must be at least " + minLength + " characters.");

            if (totalCaps < minCaps)
                errorMessage.AppendLine("Password must contain at least " + minCaps + " capital letters.");

            if (totalNum < minNum)
                errorMessage.AppendLine("Password must contain at least " + minNum + " numbers.");

            if (totalSpecial < minSpecial)
                errorMessage.AppendLine("Password must contain at least " + minSpecial + " special characters.");

            if (errorMessage.Length > 0)
                return errorMessage.ToString();

            _svc.SetPassword(account.ID, password);
            return null;
        }

        public void RemovePulseCheckClaims(Guid id)
        {
            _svc.RemoveClaim(id, PulseCheckClaims.PulseCheckSiteId);
            _svc.RemoveClaim(id, PulseCheckClaims.PulseCheckUserId);
            _svc.RemoveClaim(id, PulseCheckClaims.DeviceId);
            _svc.RemoveClaim(id, PulseCheckClaims.AccessToken);
        }
    }
}

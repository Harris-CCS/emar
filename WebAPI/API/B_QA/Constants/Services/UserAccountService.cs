using BrockAllen.MembershipReboot;
using Data.Repositories;
using Interfaces.Services;
using PulseCheck.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PulseCheck.Utilities;

namespace Services
{
    public class UserAccountService : UserAccountService<DomainModel.Membership.UserAccount>, IUserAccountService
    {
        private UserAccountRepository _userAccountRepo;

        static MembershipRebootConfiguration<DomainModel.Membership.UserAccount> config;
        static UserAccountService<DomainModel.Membership.UserAccount> _svc;

        public UserAccountService(UserAccountRepository repo) : base(repo)
        {
            _userAccountRepo = repo;

            config = new MembershipRebootConfiguration<DomainModel.Membership.UserAccount> {
                RequireAccountVerification = false,
                RequireAccountApproval = false,
                EmailIsUsername = true,
                EmailIsUnique = true,
            };

            _svc = new UserAccountService<DomainModel.Membership.UserAccount>(config, repo);
        }

        public DomainModel.Membership.UserAccount CreateUserAccount(DomainModel.Membership.UserAccountConfiguration newUser, string tempPassword)
        {
            var createdUser = _svc.CreateAccount(newUser.Email, tempPassword, newUser.Email);
            var finalUser = ChangeUserAccount(createdUser, newUser.Account);
            _svc.SetConfirmedEmail(createdUser.ID, newUser.Email);

            return finalUser;
        }

        public DomainModel.Membership.UserAccount EditUserAccount(DomainModel.Membership.UserAccount editedUser)
        {
            var existingUser = _svc.GetByID(editedUser.ID);
            return ChangeUserAccount(existingUser, editedUser);
        }

        public void RemoveUserAccount(DomainModel.Membership.UserAccount user)
        {
            _svc.DeleteAccount(user.ID);
        }

        

        public DomainModel.Membership.UserAccount ChangeUserAccount(DomainModel.Membership.UserAccount existing, DomainModel.Membership.UserAccount updated)
        {
            existing.LastName = updated.LastName;
            existing.FirstName = updated.FirstName;

            _svc.Update(existing);
            return existing;
        }

        public List<DomainModel.Membership.UserAccount> GetUserAccounts()
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
        public string UpdatePassword(DomainModel.Membership.UserAccount account, string password)
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

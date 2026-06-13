using System.ComponentModel.DataAnnotations;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Relational;

namespace PulseCheck.Domain.Membership
{
    public class UserAccount : RelationalUserAccount
    {
        [Display(Name = "First Name")]
        public virtual string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public virtual string LastName { get; set; }       

        public static explicit operator UserAccount(UserAccountQueryResult account)
        {
            var convertedAccount = new UserAccount
            {
                Email = account.Email,
                ID = account.ID,
                Tenant = account.Tenant,
                Username = account.Username,
            };

            return convertedAccount;
        }
    }
}
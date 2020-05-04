using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrockAllen.MembershipReboot.Ef;
using DomainModel.Membership;

namespace Data.Repositories
{
    public class UserAccountRepository : DbContextUserAccountRepository<MembershipDatabase, DomainModel.Membership.UserAccount>
    {
        private MembershipDatabase _context;
        public UserAccountRepository(MembershipDatabase ctx) : base(ctx)
        {
            _context = ctx;
        }

        public List<UserAccount> GetAllAccounts()
        {
            var accounts = _context.Users.ToList();
            return accounts;
        }
    }

    public class MembershipDatabase : MembershipRebootDbContext<DomainModel.Membership.UserAccount, DomainModel.Membership.PulseCheckGroup>
    {
        public MembershipDatabase(string name)
            : base(name)
        {
        }
    }
}

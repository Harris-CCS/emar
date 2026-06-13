using System.Collections.Generic;
using System.Linq;
using BrockAllen.MembershipReboot.Ef;
using PulseCheck.Domain.Membership;

namespace PulseCheck.Data.Repositories
{
    public class UserAccountRepository : DbContextUserAccountRepository<MembershipDatabase, UserAccount>
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

    public class MembershipDatabase : MembershipRebootDbContext<UserAccount, PulseCheckGroup>
    {
        public MembershipDatabase(string name)
            : base(name)
        {
        }
    }
}

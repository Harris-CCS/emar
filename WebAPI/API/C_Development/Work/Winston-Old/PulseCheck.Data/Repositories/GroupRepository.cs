using BrockAllen.MembershipReboot.Ef;
using PulseCheck.Domain.Membership;

namespace PulseCheck.Data.Repositories
{
    public class PulseCheckGroupRepository : DbContextGroupRepository<MembershipDatabase, PulseCheckGroup>
    {
        public PulseCheckGroupRepository(MembershipDatabase ctx)
            : base(ctx)
        {
        }
    }


}

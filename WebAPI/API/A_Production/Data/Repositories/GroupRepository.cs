using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrockAllen.MembershipReboot.Ef;
using DomainModel.Membership;

namespace Data.Repositories
{
    public class PulseCheckGroupRepository : DbContextGroupRepository<MembershipDatabase, PulseCheckGroup>
    {
        public PulseCheckGroupRepository(MembershipDatabase ctx)
            : base(ctx)
        {
        }
    }


}

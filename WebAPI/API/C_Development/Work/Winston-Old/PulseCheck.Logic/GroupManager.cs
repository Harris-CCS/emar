using BrockAllen.MembershipReboot;
using PulseCheck.Data.Repositories;
using PulseCheck.Domain.Membership;

namespace PulseCheck.Logic
{
    public class PulseCheckGroupService : GroupService<PulseCheckGroup>
    {
        /// <summary>
        /// PulseCheckGroupService constructor
        /// </summary>
        /// <param name="repo">PulseCheckGroupRepository instance</param>
        public PulseCheckGroupService(PulseCheckGroupRepository repo) : base("default", repo)
        {

        }
    }
}

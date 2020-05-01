using BrockAllen.MembershipReboot;
using Data.Repositories;
using DomainModel.Membership;

namespace Services
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

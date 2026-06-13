using BrockAllen.MembershipReboot;

namespace PulseCheck.Domain.Membership
{
    public class PulseCheckGroup : RelationalGroup
    {
        public virtual string Description { get; set; }
    }
}
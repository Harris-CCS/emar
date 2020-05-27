using PulseCheck.IDomain;

namespace PulseCheck.Domain
{
    public class StaffRole : IStaffRole
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
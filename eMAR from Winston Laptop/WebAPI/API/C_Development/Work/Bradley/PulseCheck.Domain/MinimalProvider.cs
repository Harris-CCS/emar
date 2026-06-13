using PulseCheck.IDomain;

namespace PulseCheck.Domain
{
    public class MinimalProvider : IMinimalProvider
    {
        public IStaffRole Role { get; set; }
        public IMinimalUser User { get; set; }
    }
}
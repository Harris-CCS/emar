using Interfaces.DomainModel;

namespace DomainModel
{
    public class MinimalProvider : IMinimalProvider
    {
        public IStaffRole Role { get; set; }
        public IMinimalUser User { get; set; }
    }
}
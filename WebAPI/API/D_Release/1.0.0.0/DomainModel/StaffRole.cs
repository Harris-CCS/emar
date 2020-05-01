using Interfaces.DomainModel;

namespace DomainModel
{
    public class StaffRole : IStaffRole
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}
namespace Interfaces.DomainModel
{
    public interface IMinimalProvider
    {
        IStaffRole Role { get; set; }
        IMinimalUser User { get; set; }
    }
}

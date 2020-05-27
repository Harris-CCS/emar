namespace PulseCheck.IDomain
{
    public interface IMinimalProvider
    {
        IStaffRole Role { get; set; }
        IMinimalUser User { get; set; }
    }
}

namespace Emar.Core.OutboundChart.Model
{
    public interface IMinimalProvider
    {
        IStaffRole Role { get; set; }
        IMinimalUser User { get; set; }
    }
}

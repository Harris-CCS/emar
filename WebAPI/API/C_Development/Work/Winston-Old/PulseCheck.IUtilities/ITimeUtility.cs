namespace PulseCheck.IUtilities
{
    public interface ITimeUtility
    {
        string Timestamp();
        string TimestampNoSeconds();
        string Datestamp();
    }
}

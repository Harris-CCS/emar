namespace Interfaces.Utilities
{
    public interface ITimeUtility
    {
        string Timestamp();
        string TimestampNoSeconds();
        string Datestamp();
    }
}

namespace PulseCheck.Data.Common.Database
{
    public interface IDbConnectionSettings
    {
        string ConnectionString { get; set; }
        string ProviderName { get; set; }
    }
}

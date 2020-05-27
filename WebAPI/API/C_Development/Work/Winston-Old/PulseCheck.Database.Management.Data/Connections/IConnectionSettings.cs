namespace PulseCheck.Database.Management.Data.Connections
{
    public interface IConnectionSettings
    {
        string ConnectionString { get; set; }
        string ProviderName { get; set; }
    }
}
using PulseCheck.Data.Common.Database;

namespace PulseCheck.Data.Common.Database
{
    public interface IIbexArchiveConnectionSettings : IDbConnectionSettings
    { }

    public class IbexArchiveConnectionSettings : DbConnectionSettings, IIbexArchiveConnectionSettings
    {
        public IbexArchiveConnectionSettings() { }
        public IbexArchiveConnectionSettings(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            ProviderName = providerName;
        }
    }
}

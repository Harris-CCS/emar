using PulseCheck.Data.Common.Database;

namespace PulseCheck.Data.Common.Database
{
    public interface IIbexConnectionSettings : IDbConnectionSettings
    { }

    public class IbexConnectionSettings : DbConnectionSettings, IIbexConnectionSettings
    {
        public IbexConnectionSettings() { }
        public IbexConnectionSettings(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            ProviderName = providerName;
        }
    }
}

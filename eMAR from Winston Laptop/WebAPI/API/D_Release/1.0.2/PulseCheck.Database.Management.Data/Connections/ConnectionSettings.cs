using System;

namespace PulseCheck.Database.Management.Data.Connections
{
    public class ConnectionSettings : IConnectionSettings
    {
        public ConnectionSettings()
        {
        }

        public ConnectionSettings(string connectionString, string providerName = null)
        {
            if(string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;

            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException(nameof(providerName));

            ProviderName = providerName;
        }

        public string ConnectionString { get; set;}
        public string ProviderName { get; set; }
    }
}

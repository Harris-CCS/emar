using System;

namespace PulseCheck.Data.Common.Database
{
    public class DbConnectionSettings : IDbConnectionSettings
    {
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }

        public DbConnectionSettings(){}

        public DbConnectionSettings(string connectionString, string providerName)
        {
            if(string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if(string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            ProviderName = providerName;
        }
    }
}

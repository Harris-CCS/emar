using System;
using System.Collections.Generic;
using System.Configuration;

namespace PulseCheck.Database.Management.Data.Connections
{
    public static class ConnectionFactory
    {
        private static List<string> _validConnectionNames = null;

        static ConnectionFactory()
        {
            LoadConnectionNameList();
        }

        private static void LoadConnectionNameList()
        {
            _validConnectionNames = new List<string>();

            foreach (ConnectionStringSettings s in ConfigurationManager.ConnectionStrings)
            {
                _validConnectionNames.Add(s.Name);
            }
        }

        private static bool IsConnectionNameValid(string connectionName)
        {
           return _validConnectionNames.Contains(connectionName);
        }

        public static IConnectionSettings GetConnectionSettings(string connectionName)
        {
            ConnectionSettings settings = new ConnectionSettings();
            
            if(!IsConnectionNameValid(connectionName))
                throw new ArgumentOutOfRangeException($"ConnectionName {connectionName} could not be found in the config file.");

            try
            {
                settings.ConnectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            }
            catch
            {
                throw new ArgumentOutOfRangeException($"Could not find {connectionName} in the configuration file");
            }

            try
            {
                settings.ProviderName = ConfigurationManager.ConnectionStrings[connectionName].ProviderName;
            }
            catch
            {
            }

            return !string.IsNullOrEmpty(settings.ConnectionString) ? settings : null;
        }
    }
}

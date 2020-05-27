using System;
using System.Data;
using System.Data.Common;

namespace PulseCheck.Data.Common.Database
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private IDbConnectionSettings _connectionSettings;

        public DbConnectionFactory(IDbConnectionSettings connectionSettings)
        {
            if (connectionSettings == null)
                throw new ArgumentNullException(nameof(connectionSettings));

            if (string.IsNullOrEmpty(connectionSettings.ConnectionString))
                throw new ArgumentNullException(nameof(connectionSettings.ConnectionString));

            if (string.IsNullOrEmpty(connectionSettings.ProviderName))
                throw new ArgumentNullException(nameof(connectionSettings.ProviderName));

            _connectionSettings = connectionSettings;

            try
            {
                //Validates connection string - throws if not valid
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder { ConnectionString = _connectionSettings.ConnectionString };
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException($"Invalid format for {nameof(DbConnectionSettings.ConnectionString)}: {ex.Message}");
            }
        }

        public IDbConnection Create()
        {
            var factory = DbProviderFactories.GetFactory(_connectionSettings.ProviderName);
            var dbConnection = factory.CreateConnection();

            dbConnection.ConnectionString = _connectionSettings.ConnectionString;
            return dbConnection;
        }

        public T Create<T>()
            where T: IDbConnection
        {
            var factory = DbProviderFactories.GetFactory(_connectionSettings.ProviderName);
            var dbConnection = factory.CreateConnection();

            dbConnection.ConnectionString = _connectionSettings.ConnectionString;
            return (T)(object)dbConnection;
        }

    }
}

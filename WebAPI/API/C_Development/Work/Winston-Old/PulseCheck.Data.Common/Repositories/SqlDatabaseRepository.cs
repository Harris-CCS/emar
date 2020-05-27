using System;
using PulseCheck.Data.Common.Database;

namespace PulseCheck.Data.Common.Repositories
{
    public class SqlDatabaseRepository : RepositoryBase, ISqlDatabaseRepository
    {
        protected ISqlDatabaseHandler SqlDatabaseHandler { get; private set; }

        public SqlDatabaseRepository(IDbConnectionSettings connectionSettings)
        {
            SqlDatabaseHandler = new SqlDatabaseHandler(connectionSettings);
        }

        public SqlDatabaseRepository(IDbConnectionFactory connectionFactory)
        {
            SqlDatabaseHandler = new SqlDatabaseHandler(connectionFactory);
        }

        public SqlDatabaseRepository(ISqlDatabaseHandler sqlDatabaseHandler)
        {
            if (sqlDatabaseHandler == null)
                throw new ArgumentNullException(nameof(sqlDatabaseHandler));

            SqlDatabaseHandler = sqlDatabaseHandler;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PulseCheck.Data.Common.Caching;
using PulseCheck.Data.Common.Database;
using PulseCheck.Data.Common.Repositories;

namespace PulseCheck.Archive.Data
{
    public class IbexRepository : SqlDatabaseRepository, IIbexRepository
    {
        public IbexRepository(IIbexConnectionSettings connectionSettings)
        : base(connectionSettings)
        {
        }

        public IbexRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public IbexRepository(ISqlDatabaseHandler sqlDatabaseHandler)
            : base(sqlDatabaseHandler)
        {
        }

        public void ReadOrdResultArchiveRecords(int daysBackToStart, int count)
        {
            
        }

        //private IEnumerable<string> GetProcedureFromTable(string procedureName)
        //{
        //    if (string.IsNullOrEmpty(procedureName))
        //        throw new ArgumentNullException(nameof(procedureName));

        //    DynamicParameters p = new DynamicParameters();
        //    p.Add("name", procedureName, DbType.AnsiString);

        //    return SqlDatabaseHandler.ExecuteStoredProcedure<string>("QcprProcedureGetByName", p);
        //}

    }
}

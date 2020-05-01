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
    public class IbexArchiveRepository : SqlDatabaseRepository, IIbexArchiveRepository
    {
        public IbexArchiveRepository(IIbexArchiveConnectionSettings connectionSettings)
        : base(connectionSettings)
        {
        }

        public IbexArchiveRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public IbexArchiveRepository(ISqlDatabaseHandler sqlDatabaseHandler)
            : base(sqlDatabaseHandler)
        {
        }

        public void ArchiveOrdResults(int saveDaysBack, int batchCnt = 1000)
        {
            if (batchCnt <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchCnt));

            if (saveDaysBack <= 0)
                throw new ArgumentOutOfRangeException(nameof(saveDaysBack));

            DynamicParameters p = new DynamicParameters();
            p.Add("SaveDaysBack", saveDaysBack, DbType.Int64);
            p.Add("BatchCnt", batchCnt, DbType.Int64);

            SqlDatabaseHandler.ExecuteStoredProcedure("ArchiveOrdResults", p);
        }

    }
}

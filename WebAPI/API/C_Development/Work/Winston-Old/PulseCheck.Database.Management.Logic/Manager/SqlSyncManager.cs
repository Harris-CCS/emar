using System;
using System.Collections.Generic;
using System.Linq;
using PulseCheck.Common.Logging;
using PulseCheck.Database.Management.Data.Connections;
using PulseCheck.Database.Management.Data.Repository;
using PulseCheck.Database.Management.Data.Type;
using PulseCheck.Database.Management.Logic.Type;

namespace PulseCheck.Database.Management.Logic.Manager
{
    public class SqlSyncManager : ManagerBase, ISqlSyncManager
    {
        public SqlSyncManager(ILogger logger = null)
        :base(logger)
        {
        }

        public void SyncTableColumns(SyncTableRequest request)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            if(string.IsNullOrEmpty(request.SourceConnectionStringName))
                throw new ArgumentNullException(nameof(request.SourceConnectionStringName));

            if (string.IsNullOrEmpty(request.SourceTableName))
                throw new ArgumentNullException(nameof(request.SourceTableName));

            if (string.IsNullOrEmpty(request.TargetTableName))
                throw new ArgumentNullException(nameof(request.TargetTableName));


            if (string.Equals(request.SourceTableName, request.TargetTableName,
                StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.Equals(request.SourceConnectionStringName, request.TargetConnectionStringName,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ArgumentOutOfRangeException($"Source and Target table names cannot be the same if syncronizing in the same database");
                }
            }

            if (string.IsNullOrEmpty(request.TargetConnectionStringName))
                request.SourceConnectionStringName = request.TargetConnectionStringName;

            LogInfo($"Create repository for {request.SourceConnectionStringName}");
            ISqlDDLRepository sourceRepository = new SqlDDLRepository(Logger, ConnectionFactory.GetConnectionSettings(request.SourceConnectionStringName));

            LogInfo($"Create repository for {request.TargetConnectionStringName}");
            ISqlDDLRepository targetRepository = new SqlDDLRepository(Logger, ConnectionFactory.GetConnectionSettings(request.TargetConnectionStringName));

            LogInfo($"Get Column definitions for {request.SourceTableName}");
            List<ColumnInfo> sourceTableColumns = sourceRepository.GetTableColumns(request.SourceTableName).ToList();

            LogInfo($"Add new Columns to {request.TargetTableName}");
            targetRepository.AddTableColumns(request.TargetTableName, sourceTableColumns);
        }

    }
}

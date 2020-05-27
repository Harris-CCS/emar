using System.Collections.Generic;
using PulseCheck.Database.Management.Data.Type;

namespace PulseCheck.Database.Management.Data.Repository
{
    public interface ISqlDDLRepository
    {
        IEnumerable<ColumnInfo> GetTableColumns(string tableName);
        void AddTableColumns(string tableName, IEnumerable<ColumnInfo> columns);
    }
}
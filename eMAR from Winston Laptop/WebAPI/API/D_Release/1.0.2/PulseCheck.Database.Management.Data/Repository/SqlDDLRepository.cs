using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using PulseCheck.Common.Logging;
using PulseCheck.Database.Management.Data.Connections;
using PulseCheck.Database.Management.Data.Type;

namespace PulseCheck.Database.Management.Data.Repository
{
    public class SqlDDLRepository : ISqlDDLRepository
    {
        private IConnectionSettings ConnectionSettings { get; set; }
        private ILogger Logger { get; set; }

        public SqlDDLRepository(ILogger logger, IConnectionSettings connectionSettings)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ConnectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public IEnumerable<ColumnInfo> GetTableColumns(string tableName)
        {

            if(string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            List<ColumnInfo> columns = new List<ColumnInfo>();

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionSettings.ConnectionString;
                connection.Open();

                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = $"exec sp_columns @table_name = '{tableName}'",
                    CommandType = CommandType.Text
                };

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                        columns.Add(new ColumnInfo()
                        {
                            COLUMN_NAME = reader["COLUMN_NAME"].ToString(),
                            TYPE_NAME = reader["TYPE_NAME"].ToString(),
                            PRECISION = reader["PRECISION"].ToString(),
                            IsNullable = reader["IS_NULLABLE"].ToString().ToLower() == "yes"
                        });
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Table {tableName} not found.");
                }
            }

            return columns;
        }

        public void AddTableColumns(string tableName, IEnumerable<ColumnInfo> columns)
        {
            if(string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if(columns == null)
                throw new ArgumentNullException(nameof(columns));

            List<ColumnInfo> columnList = columns.ToList();
            List<ColumnInfo> targetTableColumns = GetTableColumns(tableName).ToList();


            StringBuilder b = new StringBuilder();
            b.AppendLine($"alter table {tableName} ADD ");

            bool newColumns = false;

            List<string> columnsRequiringLength = new List<string>(){"varchar", "varbinary", "binary", "char", "nchar", "nvarchar, bigint" };

            for (int i = 0; i < columnList.Count(); i++)
            {
                ColumnInfo column = columnList[i];

                if (column.TYPE_NAME.ToLower().Contains("identity"))
                    column.TYPE_NAME = column.TYPE_NAME.TrimEnd("identity".ToCharArray());

                column.TYPE_NAME = column.TYPE_NAME.TrimEnd();

                if (!targetTableColumns.Exists(x => x.COLUMN_NAME.ToLower() == column.COLUMN_NAME.ToLower()))
                {
                    b.Append($"[{column.COLUMN_NAME}] ");
                    b.Append($"[{column.TYPE_NAME}]");

                    if (columnsRequiringLength.Contains(column.TYPE_NAME.ToLower()))
                        b.Append($"({column.PRECISION})");

                    if (!column.IsNullable)
                        b.Append($"NOT NULL");

                    if (i < columnList.Count - 1)
                        b.AppendLine(",");

                    newColumns = true;
                }
            }

            if (newColumns)
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = ConnectionSettings.ConnectionString;
                    connection.Open();

                    SqlCommand command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = b.ToString(),
                        CommandType = CommandType.Text
                    };

                    SqlDataReader reader = command.ExecuteReader();
                }

                Logger.Info($"Exec Script: {b}");
            }
        }

        
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Emar.Core.MedicationReactions
{
    /// <summary>
    /// Library to easily handle low-level DB stuff without using EF.
    /// </summary>
    public class DB : IDBUtility
    {
        /// <summary>
        /// Get the default connection string for the database
        /// </summary>
        /// <returns>Connection string</returns>
        public static string GetConnectionString()
        {
            var jsonFilename = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"appsettings.development.json")) ? @"appsettings.development.json" : @"appsettings.json";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory().Replace(@"Emar.Data", @"Emar.Api"))
                .AddJsonFile(jsonFilename)
                .Build();

            return ConfigurationExtensions.GetConnectionString(configuration, @"SqlConnection");
        }

        /// <summary>
        /// Convert a provided DataRow to a Dictionary object
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <returns>Dictionary object</returns>
        public static Dictionary<string, string> ConvertDataRowToDictionary(DataRow dr)
        {
            var dict =
                dr.Table.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]?.ToString()?.Trim());

            return dict;
        }

        private static Dictionary<string, object> ConvertDataRowToDictionaryObj(DataRow dr)
        {
            var dict =
                dr.Table.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => (object)dr[col.ColumnName]?.ToString()?.Trim());

            return dict;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of Dictionary objects
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns>List of Dictionary objects</returns>
        private static List<Dictionary<string, string>> ConvertDataSetToListOfDictionaries(DataSet ds)
        {
            var l = new List<Dictionary<string, string>>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                l.AddRange(from DataRow dr in ds.Tables[0].Rows select ConvertDataRowToDictionary(dr));
            }

            return l;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of strings
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="colName">Name of column to pull from DataSet</param>
        /// <returns>List of strings</returns>
        private static List<string> ConvertDataSetToListOfStrings(DataSet ds, string colName)
        {
            var l = new List<string>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                l.AddRange(from DataRow dr in ds.Tables[0].Rows select dr[colName]?.ToString());
            }

            return l;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of Dictionary objects
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns>List of Dictionary objects</returns>
        public static List<Dictionary<string, object>> ConvertDataSetToListOfDictionariesObj(DataSet ds)
        {
            var l = new List<Dictionary<string, object>>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                l.AddRange(from DataRow dr in ds.Tables[0].Rows select ConvertDataRowToDictionaryObj(dr));
            }

            return l;
        }

        /// <summary>
        /// Get a List of SqlParameters and a List of strings to be used for building and executing parameterized SQL
        /// </summary>
        /// <param name="paramValues">List of parameter value strings</param>
        /// <param name="paramType">SqlDbType that represents the column type in the database</param>
        /// <param name="paramPrefix">Optional prefix to use for generated SqlParameter names</param>
        /// <returns>Tuple. First item contains SqlParameter list, second item contains parameter name list</returns>
        public static Tuple<List<SqlParameter>, List<string>> GetParamsList(List<string> paramValues, SqlDbType paramType, string paramPrefix = "p")
        {
            return GetParamsList(paramValues, paramType, 0, paramPrefix);
        }

        /// <summary>
        /// Get a List of SqlParamters and a List of strings to be used for building and executing parameterized SQL
        /// </summary>
        /// <param name="paramValues">List of parameter value strings</param>
        /// <param name="paramType">SqlDbType that represents the column type in the database</param>
        /// <param name="paramSize">SqlParameter parameter size argument (ignored unless greater than 0)</param>
        /// <param name="paramPrefix">Optional prefix to use for generated SqlParameter names</param>
        /// <returns>Tuple. First item contains SqlParameter list, second item contains parameter name list</returns>
        public static Tuple<List<SqlParameter>, List<string>> GetParamsList(List<string> paramValues, SqlDbType paramType, int paramSize, string paramPrefix = "p")
        {
            var sqlParams = new List<SqlParameter>();
            var paramNames = new List<string>();

            if (paramValues != null)
            {
                var i = 1;
                foreach (var p in paramValues)
                {
                    var pName = string.Format("@{0}{1}", paramPrefix, i);
                    paramNames.Add(pName);

                    sqlParams.Add(paramSize > 0
                        ? new SqlParameter(pName, paramType, paramSize) { Value = p }
                        : new SqlParameter(pName, paramType) { Value = p });

                    i++;
                }
            }

            return new Tuple<List<SqlParameter>, List<string>>(sqlParams, paramNames);
        }

        /// <summary>
        /// Handles running SQL statements
        /// </summary>
        public abstract class SqlExecutor
        {
            /// <summary>
            /// Sql to run for command
            /// </summary>
            public string Sql;

            /// <summary>
            /// Parameters to use for command
            /// </summary>
            public SqlParameter[] Parameters;

            /// <summary>
            /// Connection to use for command
            /// </summary>
            public SqlConnection Connection;

            /// <summary>
            /// Transaction to use for command
            /// </summary>
            private SqlTransaction _transaction;

            /// <summary>
            /// Flag for whether the command is a stored procedure
            /// </summary>
            public bool IsStoredProcedure = false;

            /// <summary>
            /// Timeout for command
            /// </summary>
            private int _timeout = 0;

            private bool _madeNewConnection;

            internal SqlCommand MakeSqlCommand()
            {
                if (Connection == null)
                {
                    Connection = new SqlConnection(DB.GetConnectionString());
                    Connection.Open();
                    _madeNewConnection = true;
                }

                var command = new SqlCommand(Sql, Connection)
                {
                    CommandType = IsStoredProcedure ? CommandType.StoredProcedure : CommandType.Text,
                    CommandTimeout = _timeout,
                    Transaction = _transaction
                };

                if (Parameters != null)
                {
                    command.Parameters.AddRange(Parameters);
                }

                return command;
            }

            internal void Finish(SqlCommand cmd)
            {
                if (cmd == null)
                {
                    return;
                }

                cmd.Parameters?.Clear();

                cmd.Dispose();

                if (_madeNewConnection)
                {
                    Connection.Close();
                }
            }
        }

        /// <summary>
        /// DB select
        /// </summary>
        public class Select : SqlExecutor
        {
            /// <summary>
            /// Run the select and get a SqlDataReader
            /// </summary>
            /// <returns>SqlDataReader from command</returns>
            public SqlDataReader RunForDataReader()
            {
                SqlCommand cmd = null;

                try
                {
                    cmd = MakeSqlCommand();
                    var rdr = cmd.ExecuteReader();
                    return rdr;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Finish(cmd);
                }
            }

            /// <summary>
            /// Run the select and get a DataRow. Assumes you know you'll only get one row back.
            /// </summary>
            /// <returns>DataRow from command</returns>
            public DataRow RunForDataRow()
            {
                var ds = RunForDataSet();

                return ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 ? ds.Tables[0].Rows[0] : null;
            }

            /// <summary>
            /// Run the select and get a DataSet
            /// </summary>
            /// <returns>DataSet from command</returns>
            public DataSet RunForDataSet()
            {
                SqlCommand cmd = null;

                try
                {
                    cmd = MakeSqlCommand();
                    var sqlData = new DataSet();

                    using (var results = new SqlDataAdapter(cmd))
                    {
                        results.Fill(sqlData);
                    }

                    return sqlData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Finish(cmd);
                }
            }

            /// <summary>
            /// Run the select and get a list of Dictionary objects
            /// </summary>
            /// <returns>List of Dictionary objects</returns>
            public List<Dictionary<string, string>> RunForListOfDictionaries()
            {
                var ds = RunForDataSet();
                return ConvertDataSetToListOfDictionaries(ds);
            }

            /// <summary>
            /// Run the select and get a list of strings
            /// </summary>
            /// <param name="colName">Name of column to use from results</param>
            /// <returns>List of strings</returns>
            public List<string> RunForListOfStrings(string colName)
            {
                var ds = RunForDataSet();
                return ConvertDataSetToListOfStrings(ds, colName);
            }
        }
    }
}
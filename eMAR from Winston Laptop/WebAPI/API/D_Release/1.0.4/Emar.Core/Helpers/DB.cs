using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Emar.Core.Helpers
{
    /// <summary>
    /// Library to easily handle low-level DB stuff without using EF.
    /// </summary>
    public class DB : IDBUtility
    {
        private static IConfigurationRoot _config;
        private static string _ibexSqlConnection;
        private static string _chartingSqlConnection;
        /// <summary>
        /// Get the default connection string for the database
        /// </summary>
        /// <returns>Connection string</returns>
        public static string GetConnectionString()
        {
            if (string.IsNullOrEmpty(_ibexSqlConnection))
            {
                if (_config == null) 
                {
                    var builder = new ConfigurationBuilder();
                    BuildConfig(builder);
                    var config = builder.Build();
                    //var name = Assembly.GetCallingAssembly().GetName();
                    _config = config;
                }
                _ibexSqlConnection = _config.GetConnectionString("IbexSqlConnection");
            }

            return _ibexSqlConnection;
        }

        /// <summary>
        /// Get the charting database connection string
        /// </summary>
        /// <returns>Connection string</returns>
        public static string GetChartingConnectionString()
        {
            if (string.IsNullOrEmpty(_chartingSqlConnection))
            {
                if (_config == null)
                {
                    var builder = new ConfigurationBuilder();
                    BuildConfig(builder);
                    var config = builder.Build();
                    //var name = Assembly.GetCallingAssembly().GetName();
                    _config = config;
                }
                _chartingSqlConnection = _config.GetConnectionString("ChartingSqlConnection");
            }

            return _chartingSqlConnection;
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
        }

        /// <summary>
        /// Get the API user database connection string
        /// </summary>
        /// <returns>Connection string</returns>
        public static string GetMembershipConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PulseCheck.Membership"].ConnectionString;
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
                .ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]?.ToString().Trim());

            return dict;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of strings
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="colName">Name of column to use for Dictionary key</param>
        /// <returns>Dictionary of Dictionary that was converted from the DataSet</returns>
        public static Dictionary<string, Dictionary<string, string>> ConvertDataSetToDictionary(DataSet ds, string colName)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    var dataSetDict = ConvertDataRowToDictionary(dr);
                    dict[dataSetDict[colName]] = dataSetDict;
                }
            }
            return dict;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of strings
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="keyColName">Name of column to use for Dictionary key</param>
        /// <param name="keyValName">Name of column to use for Dictionary value</param>
        /// <returns>Dictionary that was converted from the DataSet</returns>
        public static Dictionary<string, string> ConvertDataSetToDictionary(DataSet ds, string keyColName, string valColName)
        {
            var dict = new Dictionary<string, string>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    var dataSetDict = ConvertDataRowToDictionary(dr);
                    dict[dataSetDict[keyColName]] = dataSetDict[valColName];
                }
            }
            return dict;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of Dictionary objects
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns>List of Dictionary objects</returns>
        public static List<Dictionary<string, string>> ConvertDataSetToListOfDictionaries(DataSet ds)
        {
            var l = new List<Dictionary<string, string>>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    l.Add(ConvertDataRowToDictionary(dr));
                }
            }
            return l;
        }

        /// <summary>
        /// Convert a provided DataSet to a List of strings
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="colName">Name of column to pull from DataSet</param>
        /// <returns>List of strings</returns>
        public static List<string> ConvertDataSetToListOfStrings(DataSet ds, string colName)
        {
            var l = new List<string>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    l.Add(dr[colName]?.ToString());
                }
            }
            return l;
        }

        /// <summary>
        /// Get a List of SqlParamters and a List of strings to be used for building and executing parameterized SQL
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
                    if (paramSize > 0)
                    {
                        sqlParams.Add(new SqlParameter(pName, paramType, paramSize) { Value = p });
                    }
                    else
                    {
                        sqlParams.Add(new SqlParameter(pName, paramType) { Value = p });
                    }
                    i++;
                }
            }

            return new Tuple<List<SqlParameter>, List<string>>(sqlParams, paramNames);
        }

        /// <summary>
        /// Take a string and return either the string (if non-whitespace) or the necessary DBNull.Value for a SqlParameter value
        /// </summary>
        /// <param name="p">String value</param>
        /// <returns>Object suitable for user in SqlParameter() { Value = obj }</returns>
        public static object NullParam(string p)
        {
            if (String.IsNullOrEmpty(p))
            {
                return DBNull.Value;
            }
            return p;
        }

        /// <summary>
        /// Take an int and return either the int (if greater than 0) or the necessary DBNull.Value for a SqlParameter value
        /// </summary>
        /// <param name="p">int value</param>
        /// <returns>Object suitable for user in SqlParameter() { Value = obj }</returns>
        public static object NullParam(int p)
        {
            if (p == 0)
            {
                return DBNull.Value;
            }
            return p;
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
            public SqlTransaction Transaction;

            /// <summary>
            /// Flag for whether the command is a stored procedure
            /// </summary>
            public bool IsStoredProcedure = false;

            /// <summary>
            /// Timeout for command
            /// </summary>
            public int Timeout = 0;

            private bool MadeNewConnection = false;

            internal SqlCommand MakeSqlCommand()
            {
                if (Connection == null)
                {
                    Connection = new SqlConnection(DB.GetConnectionString());
                    Connection.Open();
                    MadeNewConnection = true;
                }
                var command = new SqlCommand(Sql, Connection)
                {
                    CommandType = IsStoredProcedure ? CommandType.StoredProcedure : CommandType.Text,
                    CommandTimeout = Timeout,
                    Transaction = Transaction
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
                if (cmd.Parameters != null)
                {
                    cmd.Parameters.Clear();
                }
                cmd.Dispose();
                if (MadeNewConnection)
                {
                    Connection.Close();
                }
            }
        }

        /// <summary>
        /// DB insert
        /// </summary>
        public class Insert : SqlExecutor
        {

            /// <summary>
            /// Default run for insert - get a scalar
            /// </summary>
            /// <returns>Scalar from command result</returns>
            public int Run()
            {
                SqlCommand cmd = null;
                try
                {
                    cmd = MakeSqlCommand();
                    var resultsCnt = cmd.ExecuteNonQuery();
                    return resultsCnt;
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
        }

        /// <summary>
        /// DB update
        /// </summary>
        public class Update : SqlExecutor
        {

            /// <summary>
            /// Default run for update - get a scalar
            /// </summary>
            /// <returns>Scalar from command result</returns>
            public int Run()
            {
                SqlCommand cmd = null;
                try
                {
                    cmd = MakeSqlCommand();
                    var resultsCnt = cmd.ExecuteNonQuery();
                    return resultsCnt;
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
            /// Run the update and get a scalar
            /// </summary>
            /// <returns>Scalar from command</returns>
            public object RunForScalar()
            {
                SqlCommand cmd = null;
                try
                {
                    cmd = MakeSqlCommand();
                    var scalar = cmd.ExecuteScalar();
                    return scalar;
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
        }

        /// <summary>
        /// DB select
        /// </summary>
        public class Select : SqlExecutor
        {
            /// <summary>
            /// Default run for select - get a scalar
            /// </summary>
            /// <returns>Scalar from command result</returns>
            public object Run()
            {
                return RunForScalar();
            }

            /// <summary>
            /// Run the select and get an int - assumes first column of first row in result can be converted to an int.
            /// </summary>
            /// <returns>Int from command</returns>
            public Int32 RunForInt()
            {
                var result = RunForScalar();
                return (result != null) ? Convert.ToInt32(result.ToString()) : 0;
            }

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
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return ds.Tables[0].Rows[0];
                }
                return null;
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
            /// Run the select and get a Dictionary of Dictionary objects
            /// </summary>
            /// <param name="colName">Name of column to use for dictionary key</param>
            /// <returns>Dictionary of Dictionary objects</returns>
            public Dictionary<string, Dictionary<string, string>> RunForDictionary(string colName)
            {
                var ds = RunForDataSet();
                return ConvertDataSetToDictionary(ds, colName);
            }

            /// <summary>
            /// Run the select and get a Dictionary of results
            /// </summary>
            /// <param name="keyColName">Name of column to use for dictionary key</param>
            /// <param name="valColName">Name of column to use for dictionary key</param>
            /// <returns>Dictionary of Dictionary objects</returns>
            public Dictionary<string, string> RunForDictionary(string keyColName, string valColName)
            {
                var ds = RunForDataSet();
                return ConvertDataSetToDictionary(ds, keyColName, valColName);
            }

            /// <summary>
            /// Run the select and get a single Dictionary object
            /// </summary>
            /// <returns>Dictionary object</returns>
            public Dictionary<string, string> RunForDictionary()
            {
                var ds = RunForDataSet();
                var set = ConvertDataSetToListOfDictionaries(ds);
                if (set != null && set.Count > 0)
                {
                    return set[0];
                }
                return new Dictionary<string, string>();
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

            /// <summary>
            /// Run the select and get a scalar
            /// </summary>
            /// <returns>Scalar from command</returns>
            public object RunForScalar()
            {
                SqlCommand cmd = null;
                try
                {
                    cmd = MakeSqlCommand();
                    var scalar = cmd.ExecuteScalar();
                    return scalar;
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
        }
    }
}

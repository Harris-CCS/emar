using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PulseCheck.Utilities
{
    /// <summary>
    /// Library to handle writing DTFL entries
    /// </summary>
    public static class DTFL
    {
        /// <summary>
        /// Write a DTFL entry for a particular site
        /// </summary>
        /// <param name="site">Site ID</param>
        /// <param name="user">User ID</param>
        /// <param name="exception">SQLException encountered</param>
        /// <param name="sql">SQL string that caused DTFL</param>
        /// <param name="parameters">Parameters supplied to SQL statement, if applicable</param>
        public static void Write(byte site, int user, SqlException exception, string sql, SqlParameter[] parameters = null)
        {
            StringBuilder errorText = new StringBuilder();
            if (exception != null)
            {
                foreach (SqlError error in exception.Errors)
                {
                    errorText.AppendFormat("Message: {0}\n", error.Message)
                        .AppendFormat("Severity level: {0}\n", error.Class)
                        .AppendFormat("State: {0}\n", error.State)
                        .AppendFormat("Number: {0}\n", error.Number)
                        .AppendFormat("Procedure: {0}\n", error.Procedure)
                        .AppendFormat("Source: {0}\n", error.Source)
                        .AppendFormat("LineNumber: {0}\n", error.LineNumber);
                }
            }
            DoWrite(site, user, errorText.ToString(), sql, parameters);
        }

        /// <summary>
        /// Write a DTFL entry for a particular site
        /// </summary>
        /// <param name="site">Site ID</param>
        /// <param name="user">User ID</param>
        /// <param name="errorText">Error text</param>
        public static void Write(byte site, int user, string errorText)
        {
            DoWrite(site, user, errorText, null, null);
        }

        /// <summary>
        /// Write a DTFL entry for a particular site
        /// </summary>
        /// <param name="site">Site ID</param>
        /// <param name="user">User ID</param>
        /// <param name="errorText">Error text</param> 
        /// <param name="sql">SQL string that caused DTFL</param>
        /// <param name="parameters">Parameters supplied to SQL statement, if applicable</param>
        public static void Write(byte site, int user, string errorText, string sql, List<SqlParameter> parameters = null)
        {
            if (parameters != null)
            {
                DoWrite(site, user, errorText, sql, parameters.ToArray());
            } else
            {
                DoWrite(site, user, errorText, sql, null);
            }
        }

        /// <summary>
        /// Write a DTFL entry for a particular site
        /// </summary>
        /// <param name="site">Site ID</param>
        /// <param name="user">User ID</param>
        /// <param name="errorText">Error text</param>
        /// <param name="sql">SQL string that caused DTFL</param>
        /// <param name="parameters">Parameters supplied to SQL statement, if applicable</param>
        public static void Write(byte site, int user, string errorText, string sql, SqlParameter[] parameters = null)
        {
            DoWrite(site, user, errorText, sql, parameters);
        }

        private static void DoWrite(byte site, int user, string errorText, string sql, SqlParameter[] parameters = null)
        {
            // Look up $ROOT for site, figure out a stack trace, write to file.
            var root = new DB.Select
            {
                Sql = "SELECT root FROM org WHERE site=@site",
                Parameters = new SqlParameter[]
                {
                    new SqlParameter("@site", SqlDbType.TinyInt) { Value = site }
                }
            }.RunForScalar().ToString().Trim();

            var filePath = root + "inc\\" + site + "\\dtfl.txt";

            var paramsBuilder = new StringBuilder();
            if (parameters != null)
            {
                foreach(SqlParameter p in parameters)
                {
                    paramsBuilder.AppendFormat("\t{0} ({1}) = '{2}'\n", p.ParameterName, p.SqlDbType.ToString(), p.Value);
                }
                paramsBuilder.Append("\n");
            }

            var lines = new string[]
            {
                "=\n",
                sql + "\n",
                paramsBuilder.ToString(),
                errorText + "\n",
                DateTime.Now.ToString() + "\t" + user + "\n",
                Environment.StackTrace + "\n",
                "=\n"
            };

            FileWriter.Write(filePath, lines);
        }
    }
}
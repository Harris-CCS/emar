using System.Collections.Generic;
using System.Data;
using Dapper;
using PulseCheck.Data.Common.DataAccess;

namespace PulseCheck.Data.Common.Database
{
    public interface ISqlDatabaseHandler
    {
        IDbConnectionFactory ConnectionFactory { get; }
        int Execute(string sql, object param = null);
        int Execute(IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null);

        T QueryLastOrDefault<T>(string sql, object param = null)
            where T : IData;

        T QuerySingleOrDefault<T>(string sql, object param = null)
            where T : IData;

        IEnumerable<T> Query<T>(string sql, object param = null)
            where T : IData;

        object ExecuteScalar(string sql, object param = null);
        DynamicParameters ExecuteStoredProcedure(string spName, DynamicParameters parameters);

        IEnumerable<T> ExecuteStoredProcedure<T>(string spName, object param = null)
            where T : IData;

        void BulkInsert<T>(T[] items) where T : class, IData;

        bool Delete<T>(T value) where T : class, IData;

        bool Delete<T>(IDbConnection connection, T value, IDbTransaction transaction = null)
            where T : class, IData;

        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns>The identity Id or number if items inserted if its a list</returns>
        long Insert<T>(T value)
            where T : class, IData;

        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="value"></param>
        /// <param name="transaction"></param>
        /// <returns>The identity Id or number if items inserted if its a list</returns>
        long Insert<T>(IDbConnection connection, T value, IDbTransaction transaction = null)
            where T : class, IData;

        bool Update<T>(T value)
            where T : class, IData;

        bool Update<T>(IDbConnection connection, T value, IDbTransaction transaction = null)
            where T : class, IData;
    }
}


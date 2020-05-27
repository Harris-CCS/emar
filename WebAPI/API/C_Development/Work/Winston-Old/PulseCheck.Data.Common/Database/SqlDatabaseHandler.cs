using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using PulseCheck.Data.Common.DataAccess;
using Z.Dapper.Plus;

namespace PulseCheck.Data.Common.Database
{
    public class SqlDatabaseHandler : ISqlDatabaseHandler
    {
        private IDbConnectionSettings _connectionSettings;
        public IDbConnectionFactory ConnectionFactory { get; }

        public SqlDatabaseHandler(IDbConnectionSettings connectionSettings)
        {
            if (connectionSettings == null)
                throw new ArgumentNullException(nameof(connectionSettings));

            _connectionSettings = connectionSettings;
            ConnectionFactory = new DbConnectionFactory(connectionSettings);
        }

        public SqlDatabaseHandler(IDbConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
                throw new ArgumentNullException(nameof(connectionFactory));

            ConnectionFactory = connectionFactory;
        }

        public virtual int Execute(string sql, object param = null)
        {
            return CallWithConnection<int>((conn) => conn.Execute(sql, param));
        }

        public virtual int Execute(IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
        {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.Execute(sql, param, transaction);
        }

        public virtual T QueryLastOrDefault<T>(string sql, object param = null)
            where T : IData
        {
            return CallWithConnection<T>((conn) =>
            {
                return conn.Query<T>(sql, param).LastOrDefault();
            });
        }

        public virtual T QuerySingleOrDefault<T>(string sql, object param = null)
            where T : IData
        {
            return CallWithConnection<T>((conn) =>
            {
                return conn.Query<T>(sql, param).SingleOrDefault();
            });
        }
        public virtual IEnumerable<T> Query<T>(string sql, object param = null)
            where T : IData
        {
            return CallWithConnection<IEnumerable<T>>((conn) =>
            {
                return conn.Query<T>(sql, param);
            });
        }
        public virtual object ExecuteScalar(string sql, object param = null)
        {
            return CallWithConnection((conn) =>
            {
                return conn.ExecuteScalar(sql, param);
            });
        }

        public virtual DynamicParameters ExecuteStoredProcedure(string spName, DynamicParameters parameters)
        {
            return CallWithConnection((conn) =>
            {
                var count = conn.Execute(spName, parameters, commandType: CommandType.StoredProcedure);
                return parameters;
            });
        }

        public virtual IEnumerable<T> ExecuteStoredProcedure<T>(string spName, object param = null)
            where T: IData
        {
            return CallWithConnection((conn) =>
            {
                return conn.Query<T>(spName, param, commandType: CommandType.StoredProcedure);
            });
        }

        /// <summary>
        /// BulkInsert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void BulkInsert<T>(T[] items)
            where T : class, IData
        {
            CallWithConnection((conn) => conn.BulkInsert(items));
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns>The identity Id or number if items inserted if its a list</returns>
        public long Insert<T>(T value)
            where T: class, IData
        {
            return CallWithConnection((conn) => conn.Insert(value));
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="value"></param>
        /// <returns>The identity Id or number if items inserted if its a list</returns>
        public long Insert<T>(IDbConnection connection, T value, IDbTransaction transaction = null)
            where T : class, IData
        {
            if(connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.Insert(value, transaction);
        }

        public bool Update<T>(T value)
            where T : class, IData
        {
            return CallWithConnection((conn) => conn.Update(value));
        }

        public bool Update<T>(IDbConnection connection, T value, IDbTransaction transaction = null)
            where T : class, IData
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            return connection.Update(value, transaction);
        }

        //public bool Delete<T>(T value)
        //    where T : class, IDao
        //{
        //    return CallWithConnection((conn) => conn.Delete(value));
        //}

        private T CallWithConnection<T>(Func<SqlConnection, T> call)
        {
            using (var conn = ConnectionFactory.Create<SqlConnection>())
            {
                return call(conn);
            }
        }

        private DynamicParameters CallWithConnection(Func<SqlConnection, DynamicParameters> call)
        {
            using (var conn = ConnectionFactory.Create<SqlConnection>())
            {
                return call(conn);
            }
        }

    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PulseCheck.Common.Logging;
using PulseCheck.Database.Management.Logic.Manager;
using PulseCheck.Database.Management.Logic.Type;

namespace PulseCheck.Database.Management.Logic.Tests.Manager
{
    [TestClass()]
    public class SqlSyncManagerTests : UnitTestBase
    {
        private ISqlSyncManager _sqlTableManager;
        private SyncTableRequest _syncTableRequest;

        public SqlSyncManagerTests()
        {
            _sqlTableManager = Container.GetInstance<ISqlSyncManager>();
        }

        [TestInitialize]
        public void TestInit()
        {
            _syncTableRequest = new SyncTableRequest()
            {
                SourceConnectionStringName = "IBEX",
                TargetConnectionStringName = "IBEX_ARCHIVE",
                SourceTableName = "ord_results",
                TargetTableName = "ord_results"
            };
        }


        [TestMethod()]
        public void SqlSyncManager_Ctor()
        {
            Assert.IsNotNull(new SqlSyncManager());
        }


        [TestMethod()]
        public void SqlSyncManager_Ctor_WithLogger()
        {
            Assert.IsNotNull(new SqlSyncManager(Container.GetInstance<ILogger>()));
        }


        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod()]
        public void SyncTableColumns_Request_Null()
        {
           _sqlTableManager.SyncTableColumns(null);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod()]
        public void SyncTableColumns_Request_NullSourceConnectionStringName()
        {
            _syncTableRequest.SourceConnectionStringName = null;
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod()]
        public void SyncTableColumns_Request_ConnectionStringNameNotFound()
        {
            _syncTableRequest.SourceConnectionStringName = "not found";
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod()]
        public void SyncTableColumns_Request_NullSourceTableName()
        {
            _syncTableRequest.SourceTableName = null;
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod()]
        public void SyncTableColumns_Request_NullTargetTableName()
        {
            _syncTableRequest.TargetTableName = null;
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }

        [TestMethod()]
        public void SyncTableColumns()
        {
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod()]
        public void SyncTableColumns_Request_SameDatabaseSameTableName()
        {
            _syncTableRequest.TargetConnectionStringName = _syncTableRequest.SourceConnectionStringName;
            _syncTableRequest.TargetTableName = _syncTableRequest.SourceTableName;
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }


        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod()]
        public void SyncTableColumns_Request_TargetTableNotFound()
        {
            _syncTableRequest.TargetConnectionStringName = _syncTableRequest.SourceConnectionStringName;
            _syncTableRequest.TargetTableName = "not found";
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod()]
        public void SyncTableColumns_Request_SourceTableNotFound()
        {
            _syncTableRequest.TargetConnectionStringName = _syncTableRequest.SourceConnectionStringName;
            _syncTableRequest.SourceTableName = "not found";
            _sqlTableManager.SyncTableColumns(_syncTableRequest);
        }
    }
}
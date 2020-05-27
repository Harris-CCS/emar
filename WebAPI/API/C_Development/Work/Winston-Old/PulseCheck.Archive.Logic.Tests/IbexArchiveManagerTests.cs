using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PulseCheck.Archive.Domain;
using PulseCheck.Archive.Logic.Bindings;
using PulseCheck.Data.Common.Database;

namespace PulseCheck.Archive.Logic.Tests
{
    [TestClass]
    public class IbexArchiveManagerTests
    {
        private static readonly AutoFacLogicRegistrations _autoFacLogicRegistrations = new AutoFacLogicRegistrations();
        private static IArchiveManager _manager;
        private static IDbConnectionSettings _ibexArchiveConnectionSettings;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Bindings.AutoMapperRegistrationSingleton.Register();

            _autoFacLogicRegistrations.LoadContainer();
            _manager = _autoFacLogicRegistrations.GetType<IArchiveManager>();
        }

        [TestMethod]
        public void ManagerNotNull()
        {
            Assert.IsNotNull(_manager);
        }

        [TestMethod]
        public void Archive()
        {
            _manager.ArchiveOrdResults(220);
        }
    }
}

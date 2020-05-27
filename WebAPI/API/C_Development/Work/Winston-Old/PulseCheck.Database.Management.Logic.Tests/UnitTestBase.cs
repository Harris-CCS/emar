using Microsoft.VisualStudio.TestTools.UnitTesting;
using PulseCheck.Database.Management.Logic.Setup;
using StructureMap;

namespace PulseCheck.Database.Management.Logic.Tests
{
    [TestClass]
    public class UnitTestBase
    {
        protected IContainer Container;

        public UnitTestBase()
        {
            Container = StructureMapRegistry.ContainerFactory();
        }

    }
}

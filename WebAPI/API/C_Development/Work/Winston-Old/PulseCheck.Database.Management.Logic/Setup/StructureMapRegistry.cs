using PulseCheck.Common.Logging;
using PulseCheck.Database.Management.Logic.Manager;
using StructureMap;

namespace PulseCheck.Database.Management.Logic.Setup
{
    public class StructureMapRegistry : StructureMap.Registry
    {
        public StructureMapRegistry()
        {
            For<ILogger>().Use(c => LoggerFactory.LoggerFor(c.ParentType ?? c.RootType)).AlwaysUnique();

            ////Repositories
            //For<ISqlDDLRepository>().Use<SqlDDLRepository>();

            //Manager
            For<ISqlSyncManager>().Use<SqlSyncManager>();


        }

        public static IContainer ContainerFactory()
        {
            return new Container(x =>
            {
                x.AddRegistry(new StructureMapSettingsRegistry());
                x.AddRegistry(new StructureMapRegistry());
            });
        }
    }
}
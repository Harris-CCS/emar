using PulseCheck.Archive.Domain;
using PulseCheck.Archive.Logic.Bindings;
using PulseCheck.Common.Logging;
using PulseCheck.Database.Management.Logic.Manager;
using PulseCheck.Database.Management.Logic.Setup;
using PulseCheck.Database.Management.Logic.Type;
using System.Configuration;

namespace PulseCheck.Database.Management.Console.TableSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = StructureMapRegistry.ContainerFactory();
            AutoFacLogicRegistrations _autoFac = new AutoFacLogicRegistrations();
            _autoFac.LoadContainer();
            var archiveManager = _autoFac.GetType<IArchiveManager>();

            ILogger log = container.GetInstance<ILogger>();

            log.Info("Starting Application");

            ISqlSyncManager tableManager = container.GetInstance<ISqlSyncManager>();

            tableManager.SyncTableColumns(new SyncTableRequest()
            {
                SourceConnectionStringName = "IbexConnection",
                SourceTableName = "ord_results",
                TargetConnectionStringName = "IbexArchiveConnection",
                TargetTableName = "ord_results"
            });

            log.Info("Archiving.");
            archiveManager.ArchiveOrdResults(Data.Common.Configuration.Settings.GetInt("OrdResultsSaveDays"));

            log.Info("Completed Successfully.");

#if DEBUG
            System.Console.ReadLine();
#endif
        }
    }
}

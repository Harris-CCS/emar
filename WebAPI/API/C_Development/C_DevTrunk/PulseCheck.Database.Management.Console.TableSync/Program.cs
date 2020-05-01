using System;
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

            try
            {
                ISqlSyncManager tableManager = container.GetInstance<ISqlSyncManager>();

                log.Info("Syncing [IBEX].[ORD_RESULTS] and [IBEX_ARCHIVE].[ORD_RESULTS].");
                tableManager.SyncTableColumns(new SyncTableRequest()
                {
                    SourceConnectionStringName = "IbexConnection",
                    SourceTableName = "ord_results",
                    TargetConnectionStringName = "IbexArchiveConnection",
                    TargetTableName = "ord_results"
                });

                var saveDays = Data.Common.Configuration.Settings.GetInt("OrdResultsSaveDays");
                var batchCnt = Data.Common.Configuration.Settings.GetInt("IbexResultBatchCnt");

                log.Info($"Archiving [IBEX].[ORD_RESULTS] and [IBEX_ARCHIVE].[ORD_RESULTS].");
                log.Info($"Record Retention Days: {saveDays}");
                log.Info($"Batch Count: {batchCnt}");
                archiveManager.ArchiveOrdResults(saveDays, batchCnt);

                log.Info("Completed Successfully.");

#if DEBUG
                System.Console.ReadLine();
#endif
            }
            catch (Exception e)
            {
                var message = e.Message;

                if (e.Message.Contains("Could not find stored procedure"))
                {
                    log.Info("Could not find stored procedure 'ArchiveOrdResults'.  Only the table sync ran.");
                }
                else
                {
                    log.Info(e.Message, e);
                }
                
#if DEBUG
                System.Console.ReadLine();
#endif

            }

        }
    }
}

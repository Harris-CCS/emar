using Emar.Core.InboundData.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class IbexSqlMessageProcessorHostedService : BackgroundService
    {

        private readonly ILogger<IbexSqlMessageProcessorHostedService> _logger;
        private readonly SqlQueueNotificationChannel _channel;
        private readonly IServiceProvider _service;

        public IbexSqlMessageProcessorHostedService(ILogger<IbexSqlMessageProcessorHostedService> logger,
            SqlQueueNotificationChannel channel, IServiceProvider service)
        {
            _logger = logger;
            _channel = channel;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IbexSqlMessageProcessorHostedService Hosted Service running.");

            // We're using the channel to cue us to when there are records written to the queue.
            // However, when we are awakened by records in the channel, we're running the database queue until all records 
            // are processed.  When we get to that end-point, we are recording the top record in the database queue
            // at that point.  Any 

            // Pull the updated records out of the channel one at a time and process them
            long topRecordProcessed = 0;
            await foreach (var newQueueRecord in _channel.ReadAllAsync(stoppingToken))
            {
                if (newQueueRecord.Id <= topRecordProcessed)
                {
                    _logger.LogInformation(
                        $"Pulled Record #{newQueueRecord.Id} off of the Channel (already processed)");
                    continue;
                }
                _logger.LogInformation($"Pulled Record #{newQueueRecord.Id} off of the Channel");

                using var scope = _service.CreateScope();

                var dataProcessor = scope.ServiceProvider.GetService<IIbexIdsProcessorService>();
                if (dataProcessor == null)
                    throw new NullReferenceException("Scoped Service IbexContext not available in the DI pipeline.");

                NextQueueRecordToProcessDto record = null;
                while (dataProcessor.GetNextQueueRecordToProcess(ref record))
                {
                    //Only want to log successes when we're testing stuff.
                    //Winston Murdock, 02/19/2021.
                    //_logger.LogInformation($"Processing {record.RecordType} #{record.RecordExternalId} (pulled from the emar_update_queue)");

                    try
                    {
                        dataProcessor.ProcessUpdatedRecord(record.RecordType, record.RecordExternalId);
                        //Only want to log successes when we're testing stuff.
                        //Winston Murdock, 02/19/2021.
                        //_logger.LogInformation($"Processing {record.RecordType} #{record.RecordExternalId} succeeded.");
                    }
                    catch (Exception ex)
                    {
                        //Log any exceptions to the event viewer.
                        ////Winston Murdock, 02/21/2021.
                        //using (EventLog eventLog = new EventLog("Application"))
                        //{
                        //    string sException = "Processing " + record.RecordType.ToString() + " #" + record.RecordExternalId + " failed.\n";
                        //    sException += ex.Message + "\n";
                        //    sException += "source = " + ex.Source + "\n";
                        //    sException += ex.StackTrace + "\n";

                        //    eventLog.Source = "PulseCheck EMAR API";
                        //    eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                        //} //end using.

                    }
                }

                topRecordProcessed = record.HighestQueueIdWhenQuerying;
            }
        }

        /// <summary>
        /// override of the base class so that we can log an informational message
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmarIdsDataTransferHostedService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}

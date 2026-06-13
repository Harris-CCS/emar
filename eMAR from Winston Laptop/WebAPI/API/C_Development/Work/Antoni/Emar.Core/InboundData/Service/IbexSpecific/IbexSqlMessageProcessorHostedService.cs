using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emar.Core.InboundData.Model;
using Emar.Data.IbexEntities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class IbexSqlMessageProcessorHostedService : BackgroundService
    {
        
        private readonly ILogger<IbexSqlMessageProcessorHostedService> _logger;
        private readonly SqlQueueNotificationChannel _channel;
        private readonly IServiceProvider _service;
        private readonly Dictionary<string, long> _processedRecords = new Dictionary<string, long>();

        public IbexSqlMessageProcessorHostedService(ILogger<IbexSqlMessageProcessorHostedService> logger,
            SqlQueueNotificationChannel channel, IServiceProvider service)
        {
            _logger = logger;
            _channel = channel;
            _service = service;
        }

        #region BackgroundService Overrides

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IbexSqlMessageProcessorHostedService Hosted Service running.");

            // Pull the updated records out of the channel one at a time and process them
            await foreach (var newQueueRecord in _channel.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation($"Pulled Record #{newQueueRecord.Id} off of the Channel");

                if (RecordIsAlreadyProcessed(newQueueRecord))
                {
                    _logger.LogInformation($"Found already processed {newQueueRecord.Entity} #{newQueueRecord.ExternalId}");
                    continue;
                }

                using var scope = _service.CreateScope();

                var dataProcessor = scope.ServiceProvider.GetService<IIbexIdsProcessorService>();
                if (dataProcessor == null)
                    throw new NullReferenceException("Scoped Service IbexContext not available in the DI pipeline.");

                NextQueueRecordToProcessDto record = null;
                while (dataProcessor.GetNextQueueRecordToProcess(ref record))
                {
                    _logger.LogInformation($"Processing {record.RecordType} #{record.RecordExternalId} (pulled from the emar_update_queue");

                    dataProcessor.ProcessUpdatedRecord(record.RecordType, record.RecordExternalId);

                    UpdateAlreadyProcessedDictionary(record);
                }
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

        #endregion


        #region AlreadyProcessedRecord Maintenance

        private void UpdateAlreadyProcessedDictionary(NextQueueRecordToProcessDto record)
        {
            string dictionaryKey = CreateDictionaryKey(record.RecordType.ToString(), record.RecordExternalId);
            if (_processedRecords.ContainsKey(dictionaryKey))
                _processedRecords[dictionaryKey] = record.HighestQueueIdWhenQuerying;
            else
                _processedRecords.Add(dictionaryKey, record.HighestQueueIdWhenQuerying);
        }

        private bool RecordIsAlreadyProcessed(EmarUpdateQueue newQueueRecord)
        {
            // Loop through and remove any obsolete dictionary entries (entries where the highId < the id of the new record)
            var obsoleteRecords = _processedRecords
                .Where(r => r.Value < newQueueRecord.Id)
                .Select(r => r.Key);
            foreach (var key in obsoleteRecords) _processedRecords.Remove(key);

            // Check the new record for whether it needs to be processed
            var dictionaryKey = CreateDictionaryKey(newQueueRecord.Entity, newQueueRecord.ExternalId);

            if (!_processedRecords.TryGetValue(dictionaryKey, out long highestIdProcessed))
                return false;
            if (highestIdProcessed >= newQueueRecord.Id)
                return true;
            _processedRecords.Remove(dictionaryKey);
            return false;
        }

        private static string CreateDictionaryKey(string entity, string externalId) => entity + "\t" + externalId;

        #endregion
    }
}

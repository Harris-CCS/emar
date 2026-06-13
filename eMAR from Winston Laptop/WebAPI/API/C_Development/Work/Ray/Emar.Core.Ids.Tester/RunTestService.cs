using Emar.Core.InboundData.Model;
using Emar.Core.InboundData.Service;
using Emar.Core.InboundData.Service.IbexSpecific;
using Microsoft.Extensions.Logging;

namespace Emar.Core.Ids.Tester
{
    public class RunTestService : IRunTestService
    {
        private readonly IIbexIdsProcessorService _dataProcessor;
        private readonly ILogger<RunTestService> _logger;

        public RunTestService(IIbexIdsProcessorService dataProcessor, ILogger<RunTestService> logger)
        {
            _dataProcessor = dataProcessor;
            _logger = logger;
        }

        public void CatchUpOnQueueWork()
        {
            var dataProcessor = _dataProcessor;

            // Code to develop against
            NextQueueRecordToProcessDto record = null;
            while (dataProcessor.GetNextQueueRecordToProcess(ref record))
            {
                _logger.LogInformation($"Processing {record.RecordType} #{record.RecordExternalId} (pulled from the [emar_update_queue])");

                //Pass in the record, not the record type and record ID.
                //The "patients" method needs the actual record DTO.
                //Winston Murdock, 09/30/2022.
                dataProcessor.ProcessUpdatedRecord(record);

                // Not necessary in this context
                //UpdateAlreadyProcessedDictionary(record);
            }
        }
    }
}
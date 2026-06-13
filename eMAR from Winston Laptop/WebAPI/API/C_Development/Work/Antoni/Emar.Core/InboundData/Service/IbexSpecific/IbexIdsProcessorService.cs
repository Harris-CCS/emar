using System;
using Emar.Core.InboundData.Model;
using Emar.Core.InboundData.Model.Mappings;
using Emar.Core.InboundData.Repository;
using Emar.Core.Sites.Repository;
using Microsoft.Extensions.Logging;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class IbexIdsProcessorService : IIbexIdsProcessorService
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IIbexInboundDataRepository _inboundDataRepository;
        private readonly ILogger<IIbexIdsProcessorService> _logger;
        private readonly IIdsEmarUpdateService _idsEmarUpdateService;

        public IbexIdsProcessorService(IIbexInboundDataRepository inboundDataRepository, ISiteRepository siteRepository,
            ILogger<IIbexIdsProcessorService> logger, IIdsEmarUpdateService idsEmarUpdateService)
        {
            _siteRepository = siteRepository;
            _inboundDataRepository = inboundDataRepository;
            _logger = logger;
            _idsEmarUpdateService = idsEmarUpdateService;
        }

        public bool GetNextQueueRecordToProcess(ref NextQueueRecordToProcessDto record)
        {
            do
            {
                // Call the SP passing in the current values
                var result = _inboundDataRepository.GetNextQueueRecordToProcess(ref record);

                if (result == null)
                    return false;

                if (!Enum.TryParse(result.Entity, out InboundDataConstants.IncomingRecordType typeEnumerated))
                {
                    // TODO: Test this out with a bad entity type in the queue
                    _logger.LogError($"Found [emar_update_queue].[entity] ({result.Entity}) which was not a recognized record type");
                    continue;
                }

                // If, at this point, the record we pulled off the queue was a heartbeat record, then
                // the heartbeat has done what it was supposed to do.
                //      a) Made sure that SqlTableDependency was still functioning, and
                //      b) Got us to the point where we check the queue - in case the API went down,
                //          but records continued to accumulate.  That way, the heartbeat guarantees that
                //          we look at the queue within 60 seconds, and will see those records and process them
                // So, we don't need to go any further.
                if (typeEnumerated == InboundDataConstants.IncomingRecordType.heartbeat)
                    continue;

                record = new NextQueueRecordToProcessDto
                {
                    HighestQueueIdWhenQuerying = result.MaxId,
                    RecordType = typeEnumerated,
                    RecordExternalId = result.ExternalId
                };

                return true;
            } while (true);
        }

        public void ProcessUpdatedRecord(InboundDataConstants.IncomingRecordType entity, string externalId)
        {
            switch (entity)
            {
                case InboundDataConstants.IncomingRecordType.users:
                    ProcessUserRecord(externalId);
                    break;
                case InboundDataConstants.IncomingRecordType.patients:
                    ProcessPatientRecord(externalId);
                    break;
                case InboundDataConstants.IncomingRecordType.heartbeat:
                    // For testing the queue only - nothing to do here
                    break;
                default:
                    _logger.LogError($"From ProcessUpdateRecord(): Switch statement didn't account for emar_update_queue.entity: \"{entity}\"");
                    break;
            }
        }

        private void ProcessPatientRecord(string externalId)
        {
            var ibexPatient = _inboundDataRepository.GetPatient(externalId);
            if (ibexPatient == null)
                return;

            InboundPatientDataDto dto = IbexInboundMapper.MapPatient(ibexPatient, _siteRepository, _logger);
            if (dto == null)
                return;

            _idsEmarUpdateService.FilePatient(dto);
        }

        private void ProcessUserRecord(string externalId)
        {
            InboundUserDataDto dto = null;
            var ibexUser = _inboundDataRepository.GetUser(externalId);
            // ibexUser will be null here if the user was deleted - need to make sure the user is deactivated in emar
            if (ibexUser == null)
            {
                _idsEmarUpdateService.DeactivateUser(externalId);
                return;
            }
            
            dto = IbexInboundMapper.MapUser(ibexUser, _siteRepository, _logger);
            // dto will be null at this point if there is a field required by emar that wasn't supplied
            if (dto == null)
                return;

            _idsEmarUpdateService.FileUser(dto);
        }
    }
}
using System;
using System.Collections.Generic;
using Emar.Core.Devices.Repository;
using Emar.Core.InboundData.Model;
using Emar.Core.InboundData.Model.Mappings;
using Emar.Core.InboundData.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Sites.Repository;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;
using Emar.Data.IbexEntities;
using Microsoft.Extensions.Logging;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class IbexIdsProcessorService : IIbexIdsProcessorService
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIbexInboundDataRepository _inboundDataRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<IIbexIdsProcessorService> _logger;
        private readonly IOrderService _orderService;
        private readonly IIdsEmarUpdateService _idsEmarUpdateService;

        public IbexIdsProcessorService(IIbexInboundDataRepository inboundDataRepository, ISiteRepository siteRepository, IDeviceRepository deviceRepository,
            IUserRepository userRepository, IOrderService orderService, IIdsEmarUpdateService idsEmarUpdateService, ILogger<IIbexIdsProcessorService> logger)
        {
            _siteRepository = siteRepository;
            _userRepository = userRepository;
            _inboundDataRepository = inboundDataRepository;
            _deviceRepository = deviceRepository;
            _orderService = orderService;
            _idsEmarUpdateService = idsEmarUpdateService;
            _logger = logger;
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
                    _logger.LogError($"Found [emar_update_queue].[entity] ({result.Entity}) which was not a recognized record type");
                    typeEnumerated = InboundDataConstants.IncomingRecordType.BogusRecordType;
                }

                record = new NextQueueRecordToProcessDto
                {
                    HighestQueueIdWhenQuerying = result.MaxId,
                    RecordType = typeEnumerated,
                    RecordExternalId = result.ExternalId
                };

                // 20210108 BRM: Updated the SP to always return a row.  If nothing to process in the queue, it will
                //               return the highest queue entry in the MaxId column, .
                if (record.RecordType == InboundDataConstants.IncomingRecordType.queue_empty)
                    return false;

                // If, at this point, the record we pulled off the queue was a heartbeat record, then
                // the heartbeat has done what it was supposed to do.
                //      a) Made sure that SqlTableDependency was still functioning, and
                //      b) Got us to the point where we check the queue - in case the API went down,
                //          but records continued to accumulate.  That way, the heartbeat guarantees that
                //          we look at the queue within 60 seconds, and will see those records and process them
                // So, we don't need to go any further.
                if (typeEnumerated == InboundDataConstants.IncomingRecordType.heartbeat
                || typeEnumerated == InboundDataConstants.IncomingRecordType.BogusRecordType)
                    continue;

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
                case InboundDataConstants.IncomingRecordType.indicators:
                    ProcessPatientIndicators(externalId);
                    break;
                case InboundDataConstants.IncomingRecordType.heartbeat:
                    // For testing the queue only - nothing to do here
                    break;
                default:
                    _logger.LogError($"From ProcessUpdateRecord(): Switch statement didn't account for emar_update_queue.entity: \"{entity}\"");
                    break;
            }
        }

        private void ProcessPatientIndicators(string externalId)
        {
            var emarPatientEntity = GetEmarPatientEntity(externalId);
            if (emarPatientEntity == null)
            {
                return;
            }

            List<EmarPatientIndicatorsRetrieveView> indicators = _inboundDataRepository.GetPatientIndicators(externalId);
            if (emarPatientEntity.PatientIndicators != null)
            {
                emarPatientEntity.PatientIndicators.Clear();
            }
            foreach (EmarPatientIndicatorsRetrieveView v in indicators)
            {
                emarPatientEntity.PatientIndicators.Add(
                    new PatientIndicator
                    {
                        Code = v.Code,
                        Description = v.Description,
                        ImageName = v.ImageName,
                        OrdinalPosition = v.OrdinalPosition,
                        PatientId = emarPatientEntity.Id,
                        Patient = emarPatientEntity,
                        Type = v.Type,
                        TypeDescription = v.TypeDescription
                    }
                );
            }

            _idsEmarUpdateService.FilePatientIndicators(emarPatientEntity);
        }

        private void ProcessPatientRecord(string externalId)
        {
            var emarPatientEntity = GetEmarPatientEntity(externalId);
            if (emarPatientEntity == null)
            {
                return;
            }

            _inboundDataRepository.AddPatientAllergies(emarPatientEntity);
            _inboundDataRepository.AddPatientHomeMedications(emarPatientEntity);

            List<EmarPersonnelRetrieveView> result = _inboundDataRepository.GetPatientUsers(externalId);
            if (emarPatientEntity.UserPatients != null) {
                emarPatientEntity.UserPatients.Clear();
            }
            foreach (EmarPersonnelRetrieveView v in result)
            {
                if (v.ExternalUserId == 0)
                {
                    continue;
                }

                var emarUserId = _userRepository.GetInternalUserId(v.ExternalUserId.ToString());
                emarPatientEntity.UserPatients.Add(
                    new UserPatient {
                        PatientId = emarPatientEntity.Id,
                        RoleName = v.RoleName,
                        UserId = emarUserId
                    }
                );
            }

            _idsEmarUpdateService.FilePatient(emarPatientEntity);

            // With new information filed, recalculate interactions and reactions on existing orders.
            _orderService.UpdatePatientOrderInteractionsAndReactions(emarPatientEntity.Id);
        }

        private void ProcessUserRecord(string externalId)
        {
            var ibexUser = _inboundDataRepository.GetUser(externalId);
            // ibexUser will be null here if the user was deleted - need to make sure the user is deactivated in emar
            if (ibexUser == null)
            {
                _idsEmarUpdateService.DeactivateUser(externalId);
                return;
            }

            var userEntity = IbexInboundMapper.MapUser(ibexUser, _deviceRepository, _siteRepository, _userRepository, _logger);
            // dto will be null at this point if there is a field required by emar that wasn't supplied
            if (userEntity == null)
                return;

            _idsEmarUpdateService.FileUser(userEntity);
        }

        private Patient GetEmarPatientEntity(string externalId)
        {
            // Parse the External ID (site/ibex)
            var idParts = externalId.Split("|");
            if (idParts.Length != 2)
            {
                _logger.LogError($"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which was not parseable into 2 parts (site and ibex).");
                return null;
            }
            if (!long.TryParse(idParts[1], out long tempIbexNum))
            {
                _logger.LogError(
                    $"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which had a second part (ibex) that was not parseable into a long.");
                return null;
            }

            var ibexPatient = _inboundDataRepository.GetPatient(idParts[1]);
            if (ibexPatient == null)
            {
                //The patient isn't in the pat table in ibex.
                //Pull them from the hst table.
                var ibexArchivedPatient = _inboundDataRepository.GetArchivedPatient(idParts[1]);
                if (ibexArchivedPatient != null)
                {
                    ibexPatient = IbexInboundMapper.MapArchivedPatientToPatient(ibexArchivedPatient);
                } //end if

                //Tenative change pending confirmation from Romel.
                //When the patient isn't in the pat table, then we want to deactivate the patient.
                //Winston Murdock, 06/23/2021.  EMAR-936.
                _idsEmarUpdateService.DeactivatePatient(externalId);

                //Possibly don't need to do this.
                //But just in case, we save the patient to the DB context later on,
                //let's flip the active flag to false here.
                //Winston Murdock, 06/23/2021.  EMAR-936.
                if (ibexPatient != null)
                {
                    ibexPatient.IsActive = 0;
                } //end if
            } //end if (ibexPatient == null?)

            //We were only deactivating patients when we couldn't find them in pat or hst.
            //But a patient must always be in one or the other.
            //So I don't know how we were deactivating patients prior to the above change.
            //I've added the logic above to deactivate the patient when they're not in pat.
            //But it doesn't hurt to leave this in here.  So I will leave it.
            //Winston Murdock, 06/23/2021.  EMAR-936.
            if (ibexPatient == null)
            {
                // To deactivate the patient, we need the full site|ibex...
                _idsEmarUpdateService.DeactivatePatient(externalId);
                return null;
            }

            // Result will be null at this point if there is a validation error in the mapper
            return IbexInboundMapper.MapPatient(ibexPatient, _siteRepository, _logger);
        }
    }
}
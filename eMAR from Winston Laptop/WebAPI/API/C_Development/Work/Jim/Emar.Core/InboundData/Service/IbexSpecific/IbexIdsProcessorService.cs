using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Devices.Repository;
using Emar.Core.InboundData.Model;
using Emar.Core.InboundData.Model.Mappings;
using Emar.Core.InboundData.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Sites.Repository;
using Emar.Core.Users.Repository;
using Emar.Data.Entities;
using Emar.Data.IbexEntities;
using Microsoft.EntityFrameworkCore.Internal;
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
                    RecordExternalId = result.ExternalId,
                    QueueRecordId = result.QueueRecordId
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

        public void ProcessUpdatedRecord(NextQueueRecordToProcessDto record)
        {
            switch (record.RecordType)
            {
                case InboundDataConstants.IncomingRecordType.users:
                    ProcessUserRecord(record.RecordExternalId);
                    break;
                case InboundDataConstants.IncomingRecordType.patients:
                    ProcessPatientRecord(record);
                    break;
                case InboundDataConstants.IncomingRecordType.indicators:
                    ProcessPatientIndicators(record.RecordExternalId);
                    break;
                case InboundDataConstants.IncomingRecordType.heartbeat:
                    // For testing the queue only - nothing to do here
                    break;
                default:
                    _logger.LogError($"From ProcessUpdateRecord(): Switch statement didn't account for emar_update_queue.entity: \"{record.RecordType}\"");
                    break;
            }
        }

        private void ProcessPatientIndicators(string externalId)
        {
            // TODO: BRM-Work through the following - make sure it doesn't drop allergies or meds
            var ibexPatientEntity = GetIbexPatient(externalId);
            if (ibexPatientEntity == null)
                return;

            var emarPatientEntity = IbexInboundMapper.MapPatient(ibexPatientEntity, _siteRepository, _logger);
            List<EmarPatientIndicatorsRetrieveView> indicators = _inboundDataRepository.GetPatientIndicators(externalId);
            if (emarPatientEntity.PatientIndicators != null && emarPatientEntity.PatientIndicators.Count > 0)
                emarPatientEntity.PatientIndicators.Clear();

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

        private void ProcessPatientRecord(NextQueueRecordToProcessDto record)
        {
            var externalId = record.RecordExternalId;

            var ibexPatientEntity = GetIbexPatient(externalId);
            if (ibexPatientEntity != null)
            {
                var emarPatientEntity = IbexInboundMapper.MapPatient(ibexPatientEntity, _siteRepository, _logger);

                // Fire off the SP which will calculate the medication_id's for this patient's Allergies and Home Meds
                _inboundDataRepository.FireMedicationIdCalculationSp(ibexPatientEntity.ExternalId);

                try
                {
                    _inboundDataRepository.AddPatientAllergies(emarPatientEntity);
                }
                catch (Exception ex)
                {
                    // 20220629 BRM: We don't want this to stop us from filling the patient
                    _inboundDataRepository.LogQueueError(record.QueueRecordId, "ProcessPatientRecord() - AddPatientAllergies", ex);
                }

                try
                {
                    _inboundDataRepository.AddPatientHomeMedications(emarPatientEntity);
                }
                catch (Exception ex)
                {
                    // 20220629 BRM: We don't want this to stop us from filling the patient
                    _inboundDataRepository.LogQueueError(record.QueueRecordId, "ProcessPatientRecord() - AddPatientHomeMedications", ex);
                }

                List<EmarPersonnelRetrieveView> result = _inboundDataRepository.GetPatientUsers(externalId);
                if (emarPatientEntity.UserPatients != null && emarPatientEntity.UserPatients.Count > 0)
                {
                    emarPatientEntity.UserPatients.Clear();
                }
                emarPatientEntity.UserPatients ??= new List<UserPatient>();
                foreach (EmarPersonnelRetrieveView v in result.Where(u => u.ExternalUserId != 0))
                {
                    emarPatientEntity.UserPatients.Add(
                        new UserPatient
                        {
                            PatientId = emarPatientEntity.Id,
                            RoleName = v.RoleName,
                            UserId = _userRepository.GetInternalUserId(v.ExternalUserId.ToString())
                        }
                    );
                }
                _idsEmarUpdateService.FilePatient(emarPatientEntity, out bool interactRecalcNeeded, record.QueueRecordId);

                // With new information filed, recalculate interactions and reactions on existing orders.
                if (interactRecalcNeeded)
                    _orderService.UpdatePatientOrderInteractionsAndReactions(emarPatientEntity.Id, null, true);
            }
            else
                _idsEmarUpdateService.DeactivatePatient(externalId);
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

        private EmarPatientsRetrieveView GetIbexPatient(string externalId)
        {
            // Parse the External ID (site/ibex)
            var idParts = externalId.Split("|");
            if (idParts.Length != 2)
            {
                _logger.LogError(
                    $"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which was not parseable into 2 parts (site and ibex).");
                return null;
            }

            if (!long.TryParse(idParts[1], out long tempIbexNum))
            {
                _logger.LogError(
                    $"Found [external_id] in [emar_update_queue] for [entity] = 'patients' ({externalId}) which had a second part (ibex) that was not parseable into a long.");
                return null;
            }

            EmarPatientsRetrieveView ibexPatient = _inboundDataRepository.GetPatient(idParts[1]);
            if (ibexPatient == null)
            {
                //The patient isn't in the pat table in ibex.
                //Pull them from the hst table.
                EmarArchivedPatientsRetrieveView ibexArchivedPatient =
                    _inboundDataRepository.GetArchivedPatient(idParts[1]);
                if (ibexArchivedPatient == null)
                    return null;

                ibexPatient = IbexInboundMapper.MapArchivedPatientToPatient(ibexArchivedPatient);
            }

            return ibexPatient;
        }
    }
}
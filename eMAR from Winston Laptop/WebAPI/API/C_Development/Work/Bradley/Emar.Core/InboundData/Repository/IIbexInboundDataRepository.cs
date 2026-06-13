using System;
using System.Collections.Generic;
using Emar.Core.InboundData.Model;
using Emar.Data.Entities;
using Emar.Data.IbexEntities;

namespace Emar.Core.InboundData.Repository
{
    public interface IIbexInboundDataRepository
    {
        EmarUpdateQueueMaintenance GetNextQueueRecordToProcess(ref NextQueueRecordToProcessDto queueRecord);
        EmarUsersRetrieveView GetUser(string externalId);
        EmarPatientsRetrieveView GetPatient(string externalId);
        List<EmarPatientIndicatorsRetrieveView> GetPatientIndicators(string externalId);
        List<EmarPersonnelRetrieveView> GetPatientUsers(string externalId);
        EmarArchivedPatientsRetrieveView GetArchivedPatient(string externalId);
        void AddPatientAllergies(Patient emarPatientEntity);
        void AddPatientHomeMedications(Patient emarPatientEntity);
        void LogQueueError(string queueRecordId, string errorLocation, Exception exception);
        public void FireMedicationIdCalculationSp(string externalPatientId);
    }
}

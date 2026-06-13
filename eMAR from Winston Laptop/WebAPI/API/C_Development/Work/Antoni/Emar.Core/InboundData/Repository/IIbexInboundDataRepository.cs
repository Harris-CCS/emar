using Emar.Core.InboundData.Model;
using Emar.Data.IbexEntities;

namespace Emar.Core.InboundData.Repository
{
    public interface IIbexInboundDataRepository
    {
        EmarUpdateQueueMaintenance GetNextQueueRecordToProcess(ref NextQueueRecordToProcessDto queueRecord);
        EmarUsersRetrieveView GetUser(string externalId);
        EmarPatientsRetrieveView GetPatient(string externalId);
    }
}

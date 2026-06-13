using Emar.Core.InboundData.Model;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public interface IIbexIdsProcessorService
    {
        bool GetNextQueueRecordToProcess(ref NextQueueRecordToProcessDto record);
        void ProcessUpdatedRecord(NextQueueRecordToProcessDto record);
    }
}

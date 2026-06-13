namespace Emar.Core.InboundData.Model
{
    public class NextQueueRecordToProcessDto
    {
        public long HighestQueueIdWhenQuerying { get; set; }
        public InboundDataConstants.IncomingRecordType RecordType { get; set; }
        public string RecordExternalId { get; set; }
        //The id of the record we are processing from the emar_update_queue table.
        //I also had to add him to the emar_update_queue_maintenance SP
        //and the EmarUpdateQueueMaintenance entity.
        //Winston Murdock, 12/07/2021.  PC-26824
        public string QueueRecordId { get; set; }
    }
}

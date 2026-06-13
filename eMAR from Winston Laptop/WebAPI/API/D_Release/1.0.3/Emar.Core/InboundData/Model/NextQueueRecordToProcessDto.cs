namespace Emar.Core.InboundData.Model
{
    public class NextQueueRecordToProcessDto
    {
            public long HighestQueueIdWhenQuerying { get; set; }
            public InboundDataConstants.IncomingRecordType RecordType { get; set; }
            public string RecordExternalId { get; set; }
    }
}

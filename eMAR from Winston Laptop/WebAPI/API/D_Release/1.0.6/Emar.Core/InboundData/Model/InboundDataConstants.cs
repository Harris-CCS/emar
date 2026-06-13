namespace Emar.Core.InboundData.Model
{
    public class InboundDataConstants
    {
        public enum IncomingRecordType
        {
            users,
            patients,
            indicators,
            heartbeat,
            queue_empty,
            BogusRecordType
        }

        internal const string HeartbeatLabel = "heartbeat";
    }
}

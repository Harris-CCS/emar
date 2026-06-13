using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.InboundData.Model
{
    public class InboundDataConstants
    {
        public enum IncomingRecordType
        {
            users,
            patients,
            heartbeat
        }

        internal const string HeartbeatLabel = "heartbeat";
    }
}

using System.Collections.Generic;
using PulseCheck.Utilities;

namespace DomainModel
{
    /// <summary>
    /// Represents an order in the system
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Order number
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Order losecs value
        /// </summary>
        public int Losecs { get; set; }

        /// <summary>
        /// Order name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Order service number
        /// </summary>
        public string ServiceCode { get; set; }

        private Status _status;
        public string StatusCode { set
            {
                _status = new Status
                {
                    Code = value,
                    Description = OrderEntry.Constants.STATUS_CODES.ContainsKey(value) ? OrderEntry.Constants.STATUS_CODES[value] : "",
                    Style = new Style(),
                };
            }
        }

        public Status Status { get { return _status; } }

        /// <summary>
        /// Interface type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// List of codes associated with the order
        /// </summary>
        public List<Code> Codes { get; set; }

        /// <summary>
        /// List of queries associated with the order
        /// </summary>
        public List<Query> Queries { get; set; }

        /// <summary>
        /// List of events associated with the order
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// List of results associated with the order
        /// </summary>
        public List<OrderResult> Results { get; set; }

        /// <summary>
        /// User who wanted the order done
        /// </summary>
        public int OrderingPhysician { get; set; }

        /// <summary>
        /// Order quantity
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Direction of the order (inbound or outbound)
        /// </summary>
        public string Direction { get; set; } = PulseCheck.Utilities.OrderEntry.Constants.OUTBOUND_ORDER;

        /// <summary>
        /// Time in minutes from when the order was placed until it was sent
        /// </summary>
        public int SendMinutes { get; set; }

        private List<string> _errors = new List<string>();
        public List<string> Errors { get { return _errors; } }

        public void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}
using System;

namespace Emar.Core.Orders.Model
{
    public class OrderEventDto
    {
        /// <summary>
        /// Unique order event identifier
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Unique order identifier
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Unique order administration identifier
        /// </summary>
        public long? AdministrationId { get; set; }

        /// <summary>
        /// Date and time the order event took place.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset EventDateTime { get; set; }
        public string EventDate { get; set; }
        public string EventTime { get; set; }

        /// <summary>
        /// Date and time the order event was entered in the system.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset SystemDateTime { get; set; }
        public string SystemDate { get; set; }
        public string SystemTime { get; set; }

        /// <summary>
        /// Unique user identifier
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Unique order action identifier
        /// </summary>
        public int ActionId { get; set; }

        //private OrderEventAction _orderEventAction;
        ///// <summary>
        ///// Event action code
        ///// </summary>
        //public string OrderEventActionCode
        //{
        //    set
        //    {
        //        _orderEventAction = new OrderEventAction
        //        {
        //            Code = value,
        //            Description = Constants.ORDER_STATUS_CODES.ContainsKey(value) ? Constants.ORDER_STATUS_CODES[value] : String.Empty
        //        };
        //    }
        //}

        ///// <summary>
        ///// Order status
        ///// </summary>
        //public OrderEventAction OrderEventAction { get { return _orderEventAction; } }

        ///// <summary>
        ///// List of order event actions
        ///// </summary>
        //public List<OrderEventAction> OrderEventActions { get; set; }
    }
}

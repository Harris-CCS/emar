using System;
using System.Collections.Generic;
using Emar.Core.Templates.Model;
using Emar.Core.Users.Model;

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
        /// Date and time the order event took place (according to the user if they have a chance to edit).  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset EventDatetime { get; set; }

        /// <summary>
        /// Date and time the order event was entered in the system.  Includes the local time timezone offset from UTC.
        /// </summary>
        public DateTimeOffset SystemDatetime { get; set; }

        /// <summary>
        /// Unique user identifier
        /// </summary>
        internal int UserId { get; set; }

        public UserDto User { get; set; }

        /// <summary>
        /// Unique order action identifier
        /// </summary>
        internal int ActionId { get; set; }

        public ActionDto Action { get; set; }

        public int? TemplateId { get; set; }

        public IEnumerable<OrderEventDetailDto> TemplateResponses { get; set; }
    }
}

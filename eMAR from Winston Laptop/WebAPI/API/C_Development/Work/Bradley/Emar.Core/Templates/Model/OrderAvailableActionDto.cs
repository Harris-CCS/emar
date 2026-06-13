using System;
using System.Collections.Generic;
using System.Text;
using Emar.Core.Orders.Model;

namespace Emar.Core.Templates.Model
{
    class OrderAvailableActionDto
    {
        public OrderStatus OrderStatus { get; set; }
        public int AvailableActionId { get; set; }
        public ActionDto Action { get; set; }
        public bool? PointInTime { get; set; }
        public bool IsPrnOnly { get; set; }
    }
}

using Emar.Core.Orders.Model;
using Emar.Data.Entities;

namespace Emar.Core.Templates.Model
{
    internal class OrderAdministrationAvailableActionDto
    {
        public OrderStatus OrderStatus { get; set; }
        public AdministrationStatusEnum AdministrationStatus { get; set; }
        internal int AvailableActionId { get; set; }
        public ActionDto Action { get; set; }
        public bool? PointInTime { get; set; }
    }
}
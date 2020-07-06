using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Repository
{
    public interface IOrderRepository
    {
        PagedList<Order> GetOrders(long? patientId, ResourceParameters resourceParameters);
        Order GetOrder(long orderId, ResourceParameters resourceParameters);
        IEnumerable<OrderAdministration> GetAdministrations(long orderId);
        OrderAdministration GetAdministration(long administrationId);
        IEnumerable<OrderEvent> GetEvents(long orderId);
        OrderEvent GetEvent(long eventId);
        IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId);
    }
}

using System.Collections.Generic;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Repository;

namespace Emar.Core.Orders.Service
{
    public interface IOrderService
    {
        IEnumerable<OrderDto> GetOrders(long? patientId, ResourceParameters resourceParameters);
        OrderDto GetOrder(long orderId, ResourceParameters resourceParameters);
        IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId);
        OrderAdministrationDto GetAdministration(long administrationId);
        IEnumerable<OrderEventDto> GetEvents(long orderId);
        OrderEventDto GetEvent(long eventId);
        IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId);
    }
}

using System.Collections.Generic;
using Emar.Core.Orders.Model;

namespace Emar.Core.Orders.Service
{
    public interface IOrderService
    {
        PagedList<PatientOrderDto> GetOrders(long? patientId, ResourceParameters resourceParameters);
        PatientOrderDto GetOrder(long orderId, ResourceParameters resourceParameters);
        IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId);
        OrderAdministrationDto GetAdministration(long administrationId);
        IEnumerable<OrderEventDto> GetEvents(long orderId);
        OrderEventDto GetEvent(long eventId);
        IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId);
    }
}

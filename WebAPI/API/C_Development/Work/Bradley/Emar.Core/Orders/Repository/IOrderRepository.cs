using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Repository
{
    public interface IOrderRepository
    {
        PagedList<PatientOrder> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        PatientOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters);
        IEnumerable<OrderAdministration> GetAdministrations(long orderId);
        OrderAdministration GetAdministration(long administrationId);
        IEnumerable<OrderEvent> GetEvents(long orderId);
        OrderEvent GetEvent(long eventId);
        IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId);
        IEnumerable<UserQuickListItem> GetUserQuickListMostUsed(int userId, int? siteId);
        List<string> GetUserQuickListTabs(int userId, int? siteId);
        IEnumerable<UserQuickListItem> GetUserQuickListTabItems(int userId, int? siteId, string tab);
    }
}

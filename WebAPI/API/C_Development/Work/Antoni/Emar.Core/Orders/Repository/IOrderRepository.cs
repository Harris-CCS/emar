using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
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
        IEnumerable<UserQuickListItem> GetUserQuickListMostUsed(int userId, long? siteId);
        List<string> GetUserQuickListTabs(int userId, long? siteId);
        IEnumerable<UserQuickListItem> GetUserQuickListTabItems(int userId, long? siteId, string tab);
    }
}

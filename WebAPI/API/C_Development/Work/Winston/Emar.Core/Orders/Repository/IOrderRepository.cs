using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Repository
{
    public interface IOrderRepository
    {
        PagedList<PatientOrder> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        IEnumerable<PatientOrder> GetOrders(long patientId);
        PatientOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters);
        IEnumerable<OrderAdministration> GetAdministrations(long orderId);
        OrderAdministration GetAdministration(long administrationId);
        IEnumerable<OrderEvent> GetEvents(long orderId);
        OrderEvent GetEvent(long eventId);
        IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId);

        // User Quick List
        IEnumerable<UserQuickListItem> GetUserQuickListMostUsed(int userId, int? siteId);
        Dictionary<string, int> GetUserQuickListTabs(int userId, int? siteId);
        IEnumerable<UserQuickListItem> GetUserQuickListTabItems(int userId, int? siteId, string tab);
        
        // Department Preferred List
        List<DepartmentPreferredListItem> GetDepartmentPreferredList(int siteId, string departmentCode, string linkBase);

        // Group Remembered Order List
        List<GroupListItem> GetGroupRememberedOrderItems(int siteId, string departmentCode, string linkBase);
        int GetSiteForOrder(long orderId);
    }
}

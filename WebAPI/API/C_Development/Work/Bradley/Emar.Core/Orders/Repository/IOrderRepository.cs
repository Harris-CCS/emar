using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        IEnumerable<PatientOrder> GetPatientOrders(Expression<Func<PatientOrder, bool>> wherePredicate);
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
        UserQuickListItem GetUserQuickListItem(int quickListItemId);
        UserQuickListItem GetUserQuickListTabItem(long itemId, int? userId);
        FdbBrandName GetUserQuickListItemFdbBrandName(long itemId);

        // Department Preferred List
        List<DepartmentPreferredListItem> GetDepartmentPreferredList(int siteId, string departmentCode, string linkBase);
        DepartmentPreferredListItem GetDepartmentPreferredItem(long itemId);
        FdbBrandName GetDepartmentPreferredListItemFdbBrandName(long itemId);

        // Group Remembered Order List
        List<GroupListItem> GetGroupRememberedOrderItems(int siteId, string departmentCode, string linkBase);
        GroupListItem GetGroupRememberedOrderItem(long itemId);
        FdbBrandName GetGroupRememberedOrderItemFdbBrandName(long itemId);

        // Allergies
        IEnumerable<PatientAllergy> GetAllergies(Func<PatientAllergy, bool> wherePredicate);

        // Utility Methods
        int GetSiteForOrder(long orderId);
        IEnumerable<FrequencyScheduleAdministration> GetNewAdministrations(int cartOrderFrequencyId,
            DateTimeOffset start, DateTimeOffset? stop);
    }
}
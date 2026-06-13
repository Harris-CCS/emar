using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Repository
{
    public interface IOrderRepository
    {
        PagedList<PatientOrder> GetOrders(BaseLinkResource resource);
        IEnumerable<PatientOrder> GetOrders(long patientId);
        IEnumerable<PatientOrder> GetPatientOrders(Expression<Func<PatientOrder, bool>> wherePredicate);
        PatientOrder GetOrder(long orderId);
        IEnumerable<OrderAdministration> GetAdministrations(long orderId);
        OrderAdministration GetAdministration(long administrationId);
        IEnumerable<OrderEvent> GetEvents(long orderId);
        OrderEvent GetEvent(long eventId);
        IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId);

        // User Quick List
        IEnumerable<UserQuickListItem> GetUserQuickListMostUsed(BaseLinkResource resource);
        Dictionary<string, int> GetUserQuickListTabs(BaseLinkResource resource);
        IEnumerable<UserQuickListItem> GetUserQuickListTabItems(string tab, BaseLinkResource resource);
        UserQuickListItem GetUserQuickListItem(int quickListItemId);
        UserQuickListItem AddQuickListItem(UserQuickListItem item);
        UserQuickListItem GetUserQuickListTabItem(long itemId, int userId);
        FdbBrandName GetUserQuickListItemFdbBrandName(long itemId);
        bool DeleteQuickListItem(int quickListItemId);

        // Department Preferred List
        IEnumerable<DepartmentPreferredListItem> GetDepartmentPreferredList(string departmentCode, BaseLinkResource resource);
        DepartmentPreferredListItem GetDepartmentPreferredItem(long itemId);
        IEnumerable<DepartmentPreferredListItem> GetDepartmentPreferredListByTab(string tab, BaseLinkResource resource, string departmentCode);
        Dictionary<string, int> GetDepartmentPreferredListTabs(string departmentCode, BaseLinkResource resource);
        FdbBrandName GetDepartmentPreferredListItemFdbBrandName(long itemId);

        // Group Remembered Order List
        IEnumerable<GroupListItem> GetGroupRememberedOrderItems(string departmentCode, BaseLinkResource resource);
        GroupListItem GetGroupRememberedOrderItem(long itemId);
        FdbBrandName GetGroupRememberedOrderItemFdbBrandName(long itemId);

        // Allergies
        IEnumerable<PatientAllergy> GetAllergies(Func<PatientAllergy, bool> wherePredicate);

        // Scheduler Support Methods
        List<CodeSharedId> GetCodeShareSites(int siteId);
        IEnumerable<Medication> GetSchedulerSetupData(int siteId, string brandName, bool bAll);
        IEnumerable<Medication> GetSchedulerSetupData(int siteId, EmarOrderType itemType, int itemId);
        List<AntimicrobialRequiredIndicator> GetAntimicrobialRequiredIndicators(int siteId, List<Medication> medications);
        List<FrequencyScheduleAdministration> GetSchedulerAdministrations(int siteId, EmarOrderType itemType, int itemId, DateTimeOffset start, DateTimeOffset? stop, int? duration, int? durationUnitId);
        IEnumerable<OrderInstruction> GetOrderInstructions(int siteId);
        IEnumerable<FrequencySchedule> GetScheduleFrequencies(int siteId);
        IEnumerable<MedicationRoute> GetRoutes(int siteId);
        IEnumerable<MedicationUnit> GetUnits(int siteId);
        IEnumerable<DurationUnit> GetDurationUnits();

        // Utility Methods
        int GetSiteForOrder(long orderId);
        int GetSiteForAdministration(long adminId);
        List<FrequencyScheduleAdministration> GetNewAdministrations(int siteId, int frequencyId, DateTimeOffset start, DateTimeOffset? stop, int? duration, int? durationUnitId);
    }
}
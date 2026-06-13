using System;
using System.Collections.Generic;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;

namespace Emar.Core.Orders.Service
{
    public interface IOrderService
    {
        PagedList<PatientOrderDto> GetOrders(long? patientId, BaseLinkResource resource);
        IEnumerable<PatientOrderDto> GetOrders(BaseLinkResource resource);
        PatientOrderDto GetOrder(long orderId, BaseLinkResource resource);
        IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId, string administrationLinkBase);
        OrderAdministrationDto GetAdministration(long administrationId);
        IEnumerable<OrderEventDto> GetEvents(long orderId);
        OrderEventDto GetEvent(long eventId);
        IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId);

        // User Quick List services
        UserQuickListFrameworkDto GetInitialUserQuickList(BaseLinkResource resource);
        IEnumerable<UserQuickListItemDto> GetQuickListTab(string tab, BaseLinkResource resource);
        UserQuickListItemDto GetQuickListItem(int quickListItemId, BaseLinkResource resource);
        UserQuickListItemDto AddQuickListItem(UserQuickListItemAddDto quickListItemAddDto, int siteId, int userId);
        CartOrderDto CopyQuickListItemToCart(in int quickListItemId, BaseLinkResource resource, int? duration, int? durationUnitId);
        bool DeleteQuickListItem(int quickListItemId);

        // Department Preferred List services
        IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredList(string departmentCode, BaseLinkResource resource);
        IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredListByTab(string departmentCode, BaseLinkResource resource, string tabName);
        DepartmentPreferredFrameworkDto GetInitialDepartmentPreferredList(string departmentCode, BaseLinkResource resource);

        CartOrderDto CopyDepartmentPreferredItemToCart(int departmentPreferredItemId, BaseLinkResource resource);

        // Groups Remembered List services
        GroupsRememberedOrdersDto GetGroupsRememberedOrdersList(string departmentCode, BaseLinkResource resource);
        CartOrderDto CopyGroupRememberedOrderItemToCart(int groupListItemId, BaseLinkResource resource);

        //Drug Interactions & Allergy Reactions
        IEnumerable<MedicationInteractionReaction> CheckInteractionsReactions(in int userId, List<MedicationModel> medicationList, long patientId, bool checkAgainstCartOrders = true);
        void UpdatePatientOrderInteractionsAndReactions(long patientId);

        // Scheduler Support methods
        SchedulerOptionsDto GetSchedulerSetupData(int siteId, string brandName, bool bAll);
        SchedulerOptionsDto GetSchedulerSetupData(int siteId, EmarOrderType itemType, int itemId, int? duration, int? durationUnitId);
        IEnumerable<FrequencyScheduleDto> GetFrequencies(int siteId);
        IEnumerable<MedicationRouteDto> GetRoutes(int siteId);
        IEnumerable<MedicationUnitDto> GetUnits(int siteId);
        IEnumerable<FrequencyScheduleAdministrationDto> GetNewAdministrations(int siteId, int frequencyId, DateTimeOffset? start, DateTimeOffset? stop, int? duration, int? durationUnitId);
        IEnumerable<DurationUnitDto> GetDurationUnits();
    }
}
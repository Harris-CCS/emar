using System.Collections.Generic;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;

namespace Emar.Core.Orders.Service
{
    public interface IOrderService
    {
        PagedList<PatientOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        PatientOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters);
        IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId);
        OrderAdministrationDto GetAdministration(long administrationId);
        IEnumerable<OrderEventDto> GetEvents(long orderId);
        OrderEventDto GetEvent(long eventId);
        IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId);

        // User Quick List services
        UserQuickListFrameworkDto GetInitialUserQuickList(in int userId, int? siteId, string tabLinkBase, string orderLinkBase);
        IEnumerable<UserQuickListItemDto> GetQuickListTab(in int userId, int? siteId, string orderLinkBase, string tab);

        // Department Preferred List services
        IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredList(in int siteId, string departmentCode, string linkBase);

        // Groups Remembered List services
        GroupsRememberedOrdersDto GetGroupsRememberedOrdersList(int siteId, string departmentCode, string linkBase);

        // Mock Methods
        ComposerOptionsDto GetComposerSetupData(string brandName);
        IEnumerable<FrequencyDto> GetFrequencies(int siteId);
        IEnumerable<UnitDto> GetUnits(in int siteId);
    }
}

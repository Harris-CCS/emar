using System.Collections.Generic;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;

namespace Emar.Core.Orders.Service
{
    public interface IOrderService
    {
        PagedList<PatientOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        IEnumerable<PatientOrderDto> GetOrders(long patientId, string orderLinkBase, string administrationLinkBase);
        PatientOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters, string orderLinkBase, string adminLinkBase);
        IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId);
        OrderAdministrationDto GetAdministration(long administrationId);
        IEnumerable<OrderEventDto> GetEvents(long orderId);
        OrderEventDto GetEvent(long eventId);
        IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId);

        // User Quick List services
        UserQuickListFrameworkDto GetInitialUserQuickList(in int userId, int? siteId, string tabLinkBase, string orderLinkBase);
        IEnumerable<UserQuickListItemDto> GetQuickListTab(in int userId, int? siteId, long patientId, string orderLinkBase, string tab);
        CartOrderDto CopyQuickListItemToCart(in int userId, in int quickListItemId, long patientId);

        // Department Preferred List services
        IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredList(in int siteId, string departmentCode, string linkBase);
        CartOrderDto CopyDepartmentPreferredItemToCart(in int userId, int departmentPreferredItemId, long patientId);

        // Groups Remembered List services
        GroupsRememberedOrdersDto GetGroupsRememberedOrdersList(int siteId, string departmentCode, string linkBase);
        CartOrderDto CopyGroupRememberedOrderItemToCart(in int userId, int groupListItemId, long patientId);

        // Mock Methods
        ComposerOptionsDto GetComposerSetupData(string brandName);
        IEnumerable<MockFrequencyDto> GetFrequencies(int siteId);
        IEnumerable<MockUnitDto> GetUnits(in int siteId);
        ActionResultDto FireActionAgainstOrder(in int orderId, string actionCode);
        ActionResultDto FireActionAgainstAdministration(in int administrationId, string actionCode);
    }
}
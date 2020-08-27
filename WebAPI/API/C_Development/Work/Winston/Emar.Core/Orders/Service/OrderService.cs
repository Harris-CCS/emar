using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Service
{
    public partial class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOptionRepository _optionRepository;

        public OrderService(IOrderRepository orderRepository, IOptionRepository optionRepository)
        {
            _orderRepository = orderRepository;
            _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
        }

        public PagedList<PatientOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            var orders = _orderRepository.GetOrders(patientId, resourceParameters);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            var dateFormat = _optionRepository.GetOption(orders[0].Patient.SiteId, AppConstants.LongDateFormat);

            var ordersList = orders.Select(order => OrderMapper.MapOrder(order, dateFormat)).ToList();

            return new PagedList<PatientOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public PatientOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            var order = _orderRepository.GetOrder(orderId, resourceParameters);

            if (order == null)
            {
                return null;
            }

            int siteId = order.Patient.SiteId;
            var dateFormat = _optionRepository.GetOption(siteId, AppConstants.LongDateFormat);

            var orderDto = OrderMapper.MapOrder(order, dateFormat);

            return orderDto;
        }

        public IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId)
        {
            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(orderId);
            var dateFormat = _optionRepository.GetOption(siteId, AppConstants.LongDateFormat);
            var administrations = _orderRepository.GetAdministrations(orderId);

            return administrations
                .Select(administration => OrderMapper.MapOrderAdministration(administration, dateFormat)).ToList();
        }

        public OrderAdministrationDto GetAdministration(long administrationId)
        {
            var administration = _orderRepository.GetAdministration(administrationId);
            var siteId = _orderRepository.GetSiteForOrder(administration.PatientOrderId);
            var administrationDto = OrderMapper.MapOrderAdministration(administration,
                _optionRepository.GetOption(siteId, AppConstants.LongDateFormat));

            return administrationDto;
        }

        public IEnumerable<OrderEventDto> GetEvents(long orderId)
        {
            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(orderId);
            var dateFormat = _optionRepository.GetOption(siteId, AppConstants.LongDateFormat);

            var events = _orderRepository.GetEvents(orderId);

            return events.Select(@event => OrderMapper.MapOrderEvent(@event, dateFormat)).ToList();
        }

        public OrderEventDto GetEvent(long eventId)
        {
            var @event = _orderRepository.GetEvent(eventId);

            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(@event.PatientOrderId);
            var dateFormat = _optionRepository.GetOption(siteId, AppConstants.LongDateFormat);

            var eventDto = OrderMapper.MapOrderEvent(@event, dateFormat);

            return eventDto;
        }

        public IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId)
        {
            var events = _orderRepository.GetAdministrationEvents(administrationId).ToList();

            string dateFormat = "";
            if (events.Any())
            {
                // Get the Site's LongDateFormat Option
                var siteId = _orderRepository.GetSiteForOrder(events[0].PatientOrderId);
                dateFormat = _optionRepository.GetOption(siteId, AppConstants.LongDateFormat);
            }

            return events.Select(@event => OrderMapper.MapOrderEvent(@event, dateFormat)).ToList();
        }

        #region User Quick List Services

        public UserQuickListFrameworkDto GetInitialUserQuickList(in int userId, int? siteId, 
            string tabLinkBase, string orderLinkBase)
        {
            List<string> tabList = _orderRepository.GetUserQuickListTabs(userId, siteId).OrderBy(i => i).ToList();
            // Compress all non-alpha values into "#"
            if (!tabList.Any())
                return null;

            var foundNonAlpha = false;
            for (int i = tabList.Count - 1; i >= 0; i--)
            {
                if (!char.IsLetter(Convert.ToChar(tabList[i])))
                {
                    foundNonAlpha = true;
                    tabList.RemoveAt(i);
                }
            }
            if (foundNonAlpha)
                tabList.Add("#");

            var mostUsedItems = _orderRepository.GetUserQuickListMostUsed(userId, siteId).ToList();
            List<UserQuickListItemDto> firstTabContents;
            if (mostUsedItems.Any())
            {
                firstTabContents = mostUsedItems.Select(item => OrderMapper.MapUserQuickListItem(item, orderLinkBase))
                    .OrderBy(i => i.BrandName).ToList();
                tabList.Insert(0, Constants.MostUsedTabTitle);
            }
            else
            {
                var items = _orderRepository.GetUserQuickListTabItems(userId, siteId, tabList[0]).ToList();

                firstTabContents = items.Select(dbObj => OrderMapper.MapUserQuickListItem(dbObj, orderLinkBase))
                    .OrderBy(i => i.BrandName).ToList();
            }
            var ret = new UserQuickListFrameworkDto(firstTabContents, tabList, tabLinkBase);

            return ret;
        }

        public IEnumerable<UserQuickListItemDto> GetQuickListTab(in int userId, int? siteId, string orderLinkBase,
            string tab)
        {
            List<UserQuickListItem> tabItems;
            tabItems = tab == Constants.MostUsedTabTitle
                ? _orderRepository.GetUserQuickListMostUsed(userId, siteId).ToList()
                : _orderRepository.GetUserQuickListTabItems(userId, siteId, tab).ToList();

            if (!tabItems.Any())
                return null;

            return tabItems.Select(item => OrderMapper.MapUserQuickListItem(item, orderLinkBase))
                .OrderBy(i => i.BrandName);
        }

        #endregion

        #region Department Preferred List Services

        public IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredList(in int siteId, string departmentCode,
            string linkBase)
        {
            List<DepartmentPreferredListItem> orders = _orderRepository.GetDepartmentPreferredList(siteId,
                departmentCode,
                linkBase).ToList();

            if (!orders.Any()) return null;

            return orders.Select(item => OrderMapper.MapDepartmentPreferredListItem(item, linkBase))
                .OrderBy(i => i.BrandName);
        }

        #endregion

        #region Group Remembered Order Services

        public GroupsRememberedOrdersDto GetGroupsRememberedOrdersList(int siteId, string departmentCode,
            string linkBase)
        {
            List<GroupListItem> items = _orderRepository.GetGroupRememberedOrderItems(siteId, departmentCode, linkBase);
            if (!items.Any()) return null;

            var ret = new GroupsRememberedOrdersDto();
            foreach (var groupName in items.GroupBy(i => i.GroupName).Select(i => i.Key).OrderBy(i => i))
                ret.Groups.Add(new RememberedGroupDto
                {
                    GroupName = groupName,
                    Orders = items.Where(i => i.GroupName == groupName)
                        .Select(item => OrderMapper.MapGroupListItem(item, linkBase)).ToList()
                });

            return ret;
        }

        #endregion

    }
}

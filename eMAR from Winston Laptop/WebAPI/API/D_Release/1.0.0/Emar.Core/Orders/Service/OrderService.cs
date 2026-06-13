using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Carts.Repository;
using Emar.Core.Helpers;
using Emar.Core.HomeMedications.Repository;
using Emar.Core.MedicationReactions;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Medications.Repository;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Patients.Repository;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Repository;
using Emar.Core.Templates.Model.Mappings;
using Emar.Core.Templates.Repository;
using Emar.Data.Entities;
using Constants = Emar.Core.Orders.Model.Constants;

namespace Emar.Core.Orders.Service
{
    public class OrderService : IOrderService
    {
        private readonly ICartOrderRepository _cartOrderRepository;
        private readonly IHomeMedicationRepository _homeMedicationRepository;
        private readonly IInteractionRepository _interactionRepository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOptionRepository _optionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly ITemplateRepository _templateRepository;

        public OrderService(ICartOrderRepository cartOrderRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IInteractionRepository interactionRepository,
            IMedicationRepository medicationRepository,
            IPatientRepository patientRepository,
            IOptionRepository optionRepository,
            IOrderRepository orderRepository,
            ISiteRepository siteRepository,
            ITemplateRepository templateRepository)
        {
            _cartOrderRepository = cartOrderRepository ?? throw new ArgumentNullException(nameof(cartOrderRepository));
            _homeMedicationRepository = homeMedicationRepository ?? throw new ArgumentNullException(nameof(homeMedicationRepository));
            _interactionRepository = interactionRepository ?? throw new ArgumentNullException(nameof(interactionRepository));
            _medicationRepository = medicationRepository ?? throw new ArgumentNullException(nameof(MedicationRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _optionRepository = optionRepository ?? throw new ArgumentNullException(nameof(optionRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _siteRepository = siteRepository ?? throw new ArgumentNullException(nameof(siteRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        }

        public PagedList<PatientOrderDto> GetOrders(long? patientId, BaseLinkResource resourceParameters)
        {
            throw new NotImplementedException("Paging Disabled - to re-enable, make AssignNextActionTimeToOrders() work with a paged list of orders");

            //var orders = _orderRepository.GetOrders(patientId, resourceParameters);

            ////AssignNextActionTimeToOrders(orders);

            //if ((orders == null) ||
            //    (!orders.Any()))
            //{
            //    return null;
            //}

            //var siteId = orders[0].Patient.SiteId;
            //var actionHelper = new OrderActionMapperHelper(_templateRepository, siteId, null, null);
            //var dateFormat = _optionRepository.GetOption(siteId, OptionNames.SHORT_DATE_FORMAT);
            //var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);

            //var ordersList = orders.Select(order => OrderMapper.MapOrder(order, dateFormat, drugDbVendor, actionHelper, null, null, null)).ToList();

            //return new PagedList<PatientOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public IEnumerable<PatientOrderDto> GetOrders(BaseLinkResource resource)
        {
            var orders = _orderRepository.GetOrders(resource.PatientId).ToList();

            var siteId = _patientRepository.GetSiteIdForPatient(resource.PatientId);
            var actionHelper = new OrderActionMapperHelper(_templateRepository, siteId, resource.LinkExecuteOrderAction, resource.LinkExecuteAdministrationAction);

            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var retOrders = orders
                .Select(order => OrderMapper.MapOrder(order, drugDbVendor, actionHelper, codeShareSites, resource))
                .ToList()
                // sort all the orders that don't have a "Next Action Time" to the bottom of the list
                .OrderBy(o => o.NextActionTime == null ? 1 : 0)
                .ThenBy(o => o.NextActionTime)
                .ToList();

            retOrders.ForEach(r => r.OrderInteractions = r.OrderInteractions?.Distinct(new OrderInteractionDtoComparer()).ToList());

            return retOrders;
        }

        public PatientOrderDto GetOrder(long orderId, BaseLinkResource resource)
        {
            var order = _orderRepository.GetOrder(orderId);

            if (order == null)
            {
                return null;
            }

            var siteId = _patientRepository.GetSiteIdForPatient(order.PatientId);
            var actionHelper = new OrderActionMapperHelper(_templateRepository, siteId, resource.LinkExecuteOrderAction, resource.LinkExecuteAdministrationAction);

            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var orderDto = OrderMapper.MapOrder(order, drugDbVendor, actionHelper, codeShareSites, resource);

            return orderDto;
        }

        public IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId, string adminLinkBase)
        {
            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(orderId);
            var administrations = _orderRepository.GetAdministrations(orderId);

            var orderActionHelper = new OrderActionMapperHelper(_templateRepository, siteId, null, adminLinkBase);

            return administrations
                .Select(administration =>
                    OrderMapper.MapOrderAdministration(administration, OrderStatus.Pending, orderActionHelper))
                .ToList();
        }

        public OrderAdministrationDto GetAdministration(long administrationId)
        {
            var administration = _orderRepository.GetAdministration(administrationId);

            // assuming that if we are only retrieving the Administration, we won't need any links
            // Therefore passing NULL instead of an actionHelper to the mapper
            //string adminBase = null;
            //var orderActionHelper = new OrderActionMapperHelper(_templateRepository, siteId, null, adminBase);

            var administrationDto = OrderMapper.MapOrderAdministration(administration,
                OrderStatus.Pending, null /*orderActionHelper*/);

            return administrationDto;
        }

        public IEnumerable<OrderEventDto> GetEvents(long orderId)
        {
            var events = _orderRepository.GetEvents(orderId);

            return events.Select(OrderMapper.MapOrderEvent).ToList();
        }

        public OrderEventDto GetEvent(long eventId)
        {
            var @event = _orderRepository.GetEvent(eventId);

            var eventDto = OrderMapper.MapOrderEvent(@event);

            return eventDto;
        }

        public IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId)
        {
            var events = _orderRepository.GetAdministrationEvents(administrationId).ToList();

            return events.Select(OrderMapper.MapOrderEvent).ToList();
        }

        #region User Quick List Services
        public UserQuickListFrameworkDto GetInitialUserQuickList(BaseLinkResource resource)
        {
            var tabList = _orderRepository.GetUserQuickListTabs(resource).OrderBy(i => i.Key).ToList();

            if (!tabList.Any())
            {
                return null;
            }

            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            // Compress all non-alpha values into "#"
            var numAlphas = 0;

            for (var i = tabList.Count - 1; i >= 0; i--)
            {
                if (!char.IsLetter(Convert.ToChar(tabList[i].Key)))
                {
                    numAlphas += tabList[i].Value;
                    tabList.RemoveAt(i);
                }
            }

            if (numAlphas > 0)
            {
                tabList.Add(new KeyValuePair<string, int>("#", numAlphas));
            }

            var mostUsedItems = _orderRepository.GetUserQuickListMostUsed(resource).ToList();

            List<UserQuickListItemDto> firstTabContents;

            if (mostUsedItems.Any())
            {
                firstTabContents = mostUsedItems
                    .Select(item => OrderMapper.MapUserQuickListItem(item, codeShareSites, resource))
                    .OrderBy(i => i.Medication.DisplayName)
                    .ToList();

                tabList.Insert(0, new KeyValuePair<string, int>(Constants.MostUsedTabTitle, mostUsedItems.Count));
            }
            else
            {
                var items = _orderRepository.GetUserQuickListTabItems(tabList[0].Key, resource).ToList();

                firstTabContents = items
                    .Select(item => OrderMapper.MapUserQuickListItem(item, codeShareSites, resource))
                    .OrderBy(i => i.Medication.DisplayName)
                    .ToList();
            }

            firstTabContents = AddInteractionsReactions(firstTabContents.ToList(), resource);

            return new UserQuickListFrameworkDto(firstTabContents, tabList, resource.LinkGetUserQuickListTab);
        }

        public IEnumerable<UserQuickListItemDto> GetQuickListTab(string tab, BaseLinkResource resource)
        {
            var tabItems = tab == Constants.MostUsedTabTitle
                ? _orderRepository.GetUserQuickListMostUsed(resource).ToList()
                : _orderRepository.GetUserQuickListTabItems(tab, resource).ToList();

            if (!tabItems.Any())
            {
                return null;
            }

            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            var orderedTabItems = tabItems
                .Select(item => OrderMapper.MapUserQuickListItem(item, codeShareSites, resource))
                .OrderBy(i => i.Medication.DisplayName)
                .ToList();

            orderedTabItems = AddInteractionsReactions(orderedTabItems.ToList(), resource);

            return orderedTabItems;
        }

        private List<UserQuickListItemDto> AddInteractionsReactions(List<UserQuickListItemDto> items, BaseLinkResource resource)
        {
            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(resource.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            var itemList = items
                .Select(item => OrderMapper.MapOrderItemDtoToModel(EmarOrderType.UserQuickListItem, item, resource.PatientId, resource.UserId))
                .ToList();

            var interactionsReactions = CheckInteractionsReactions(resource.UserId, itemList, resource.PatientId);

            foreach (var interactionReaction in interactionsReactions)
            {
                foreach (var interaction in interactionReaction.Interactions.Where(i => (i.TryGetValue("SourceTable2", out object value) ? value.ToString() : "") != SourceTables.UserQuickListItems))
                {
                    var item = items
                        .FirstOrDefault(i => i.Id == interactionReaction.SourceTableId);

                    item?.AddOrderInteraction(MedicationMapper.MapOrderInteraction(MedicationMapper.MapInteractionDictionaryToMedicationInteraction(interaction), drugDbVendor, resource, codeShareSiteMedicationUnit));
                }

                foreach (var reaction in interactionReaction.Reactions.Where(r => (r.TryGetValue("SourceTable2", out object value) ? value.ToString() : "") != SourceTables.UserQuickListItems))
                {
                    var item = items
                        .FirstOrDefault(i => i.Id == interactionReaction.SourceTableId);

                    item?.AddAllergyReaction(MedicationMapper.MapReactionDictionaryToAllergyReactionViewDto(reaction, interactionReaction));
                }
            }

            return items;
        }

        public UserQuickListItemDto GetQuickListItem(int quickListItemId, BaseLinkResource resource)
        {
            var item = _orderRepository.GetUserQuickListItem(quickListItemId);
            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            return AddInteractionsReactions(
                    new List<UserQuickListItemDto>
                    {
                        OrderMapper.MapUserQuickListItem(item, codeShareSites , resource)
                    },
                    resource)
                .FirstOrDefault();
        }

        public UserQuickListItemDto AddQuickListItem(UserQuickListItemAddDto quickListItemAddDto, int siteId, int userId)
        {
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            var item = OrderMapper.MapUserQuickListItemAddDto(quickListItemAddDto, siteId, userId);
            item = _orderRepository.AddQuickListItem(item);

            return item == null
                ? null
                : OrderMapper.MapUserQuickListItem(item, codeShareSites);
        }

        public CartOrderDto CopyQuickListItemToCart(in int quickListItemId, BaseLinkResource resource)
        {
            var quickListItem = _orderRepository.GetUserQuickListItem(quickListItemId);

            if (quickListItem == null)
            {
                return null;
            }

            var cartOrder = OrderMapper.MapUserQuickListItemToPatientCartOrder(resource.UserId, (int)resource.PatientId, quickListItem);
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrder.PatientId);
            var admins = _orderRepository.GetNewAdministrations(siteId, cartOrder.FrequencyScheduleId ?? -1,
                _siteRepository.DateTimeOffsetNow(siteId), null);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            AddEndDatetime_FixNoOfAdministrations(ref cartOrder);

            var newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var interactionsReactions = CheckInteractionsReactions(
                resource.UserId,
                new List<MedicationModel>
                {
                    OrderMapper.MapOrderItemToModel(
                        EmarOrderType.UserQuickListItem,
                        quickListItem,
                        resource.PatientId,
                        resource.UserId,
                        codeShareSites
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId)
                },
                resource.PatientId);
            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id);

            return CartOrderMapper.MapCartOrder(newCartOrder, drugDbVendor, codeShareSites, resource);
        }
        #endregion User Quick List Services

        #region Department Preferred List Services
        public IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredList(string departmentCode, BaseLinkResource resource)
        {
            var orders = _orderRepository.GetDepartmentPreferredList(departmentCode, resource)
                .ToList();

            if (!orders.Any()) return null;

            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            var itemList = orders
                .Select(item => OrderMapper.MapDepartmentPreferredListItem(item, resource, codeShareSites))
                .OrderBy(i => i.Medication.DisplayName)
                .ToList();

            itemList = AddInteractionsReactions(itemList, resource);

            return itemList;
        }

        private List<DepartmentPreferredItemDto> AddInteractionsReactions(List<DepartmentPreferredItemDto> items, BaseLinkResource resource)
        {
            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(resource.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            var itemList = items
                .Select(item => OrderMapper.MapOrderItemDtoToModel(EmarOrderType.DepartmentPreferredListItem, item, resource.PatientId, resource.UserId))
                .ToList();

            var interactionsReactions = CheckInteractionsReactions(resource.UserId, itemList, resource.PatientId);

            foreach (var interactionReaction in interactionsReactions)
            {
                foreach (var interaction in interactionReaction.Interactions.Where(i => (i.TryGetValue("SourceTable2", out object value) ? value.ToString() : "") != SourceTables.DepartmentPreferredListItems))
                {
                    var item = items
                        .FirstOrDefault(i => i.Id == interactionReaction.SourceTableId);

                    item?.AddOrderInteraction(MedicationMapper.MapOrderInteraction(MedicationMapper.MapInteractionDictionaryToMedicationInteraction(interaction), drugDbVendor, resource, codeShareSiteMedicationUnit));
                }

                foreach (var reaction in interactionReaction.Reactions.Where(r => (r.TryGetValue("SourceTable2", out object value) ? value.ToString() : "") != SourceTables.DepartmentPreferredListItems))
                {
                    var item = items
                        .FirstOrDefault(i => i.Id == interactionReaction.SourceTableId);

                    item?.AddAllergyReaction(MedicationMapper.MapReactionDictionaryToAllergyReactionViewDto(reaction, interactionReaction));
                }
            }

            return items;
        }

        public CartOrderDto CopyDepartmentPreferredItemToCart(int departmentPreferredItemId, BaseLinkResource resource)
        {
            var departmentPreferredListItem = _orderRepository.GetDepartmentPreferredItem(departmentPreferredItemId);

            if (departmentPreferredListItem == null)
            {
                return null;
            }

            var cartOrder = OrderMapper.MapDepartmentPreferredListItemToPatientCartOrder(resource.UserId, resource.PatientId, departmentPreferredListItem);
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrder.PatientId);
            var admins = _orderRepository.GetNewAdministrations(siteId, cartOrder.FrequencyScheduleId ?? -1,
                _siteRepository.DateTimeOffsetNow(siteId), null);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            AddEndDatetime_FixNoOfAdministrations(ref cartOrder);

            var newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var interactionsReactions = CheckInteractionsReactions(
                resource.UserId,
                new List<MedicationModel>
                {
                    OrderMapper.MapOrderItemToModel(
                        EmarOrderType.DepartmentPreferredListItem,
                        departmentPreferredListItem,
                        resource.PatientId,
                        resource.UserId,
                        codeShareSites
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId)
                },
                resource.PatientId);
            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id);

            return CartOrderMapper.MapCartOrder(newCartOrder, drugDbVendor, codeShareSites, resource);
        }
        #endregion Department Preferred List Services

        #region Group Remembered Order Services
        public GroupsRememberedOrdersDto GetGroupsRememberedOrdersList(string departmentCode, BaseLinkResource resource)
        {
            var items = _orderRepository.GetGroupRememberedOrderItems(departmentCode, resource)
                .ToList();

            if (!items.Any()) return null;

            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            var itemList = items
                .Select(item => OrderMapper.MapGroupListItem(item, resource, codeShareSites))
                .ToList();

            itemList = AddInteractionsReactions(itemList, resource);

            var ret = new GroupsRememberedOrdersDto();

            foreach (var groupName in items.GroupBy(i => i.GroupName).Select(i => i.Key).OrderBy(i => i))
            {
                var groupList = itemList
                    .Where(i => i.GroupName == groupName)
                    .ToList();

                ret.Groups.Add(new RememberedGroupDto
                {
                    GroupName = groupName,
                    Orders = groupList
                });
            }

            return ret;
        }

        private List<GroupListItemDto> AddInteractionsReactions(List<GroupListItemDto> items, BaseLinkResource resource)
        {
            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(resource.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            var itemList = items
                .Select(item => OrderMapper.MapOrderItemDtoToModel(EmarOrderType.GroupRememberedOrder, item, resource.PatientId, resource.UserId))
                .ToList();

            var interactionsReactions = CheckInteractionsReactions(resource.UserId, itemList, resource.PatientId);

            foreach (var interactionReaction in interactionsReactions)
            {
                foreach (var interaction in interactionReaction.Interactions.Where(i => (i.TryGetValue("SourceTable2", out object value) ? value.ToString() : "") != SourceTables.GroupListItems))
                {
                    var item = items
                        .FirstOrDefault(i => i.Id == interactionReaction.SourceTableId);

                    item?.AddOrderInteraction(MedicationMapper.MapOrderInteraction(MedicationMapper.MapInteractionDictionaryToMedicationInteraction(interaction), drugDbVendor, resource, codeShareSiteMedicationUnit));
                }

                foreach (var reaction in interactionReaction.Reactions.Where(r => (r.TryGetValue("SourceTable2", out object value) ? value.ToString() : "") != SourceTables.GroupListItems))
                {
                    var item = items
                        .FirstOrDefault(i => i.Id == interactionReaction.SourceTableId);

                    item?.AddAllergyReaction(MedicationMapper.MapReactionDictionaryToAllergyReactionViewDto(reaction, interactionReaction));
                }
            }

            return items;
        }

        public CartOrderDto CopyGroupRememberedOrderItemToCart(int groupListItemId, BaseLinkResource resource)
        {
            var groupListItem = _orderRepository.GetGroupRememberedOrderItem(groupListItemId);

            if (groupListItem == null)
            {
                return null;
            }

            var cartOrder = OrderMapper.MapGroupListItemToPatientCartOrder(resource.UserId, resource.PatientId, groupListItem);
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrder.PatientId);
            var admins = _orderRepository.GetNewAdministrations(siteId, cartOrder.FrequencyScheduleId ?? -1,
                _siteRepository.DateTimeOffsetNow(siteId), null);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            AddEndDatetime_FixNoOfAdministrations(ref cartOrder);

            var newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var interactionsReactions = CheckInteractionsReactions(
                resource.UserId,
                new List<MedicationModel>
                {
                    OrderMapper.MapOrderItemToModel(
                        EmarOrderType.GroupRememberedOrder,
                        groupListItem,
                        resource.PatientId,
                        resource.UserId,
                        codeShareSites
                            .FirstOrDefault(c =>
                                c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                            .SharedSiteId)
                },
                resource.PatientId);
            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id);

            return CartOrderMapper.MapCartOrder(newCartOrder, drugDbVendor, codeShareSites, resource);
        }
        #endregion Group Remembered Order Services

        private static void AddEndDatetime_FixNoOfAdministrations(ref PatientCartOrder cartOrder)
        {
            //This method does two things.
            //1) It calculates the end date time for the cart order.
            //2) It calculates the scheduled stop date time for the last order
            //   administration that is earlier than the order's end date time.
            //On 03/20/2021, we were running into an issue where the cart order's end date time
            //was sometimes being set to earlier than the beginning date time.
            //The get_frequency_schedule_items SP was changed to strip off the seconds when setting
            //the start time for the first order administration.  This was needed for EMAR-847 but
            //had the unintended side effect of messing this up.
            //To get around this, we add one minute to the calculated cart order end time
            //and then use that when getting the list of order administrations earlier than
            //the cart order end time.
            //Since the actual error was calling .Last() on an empty list, I put a try/catch
            //around it to silently catch that error.  We don't need to bubble that error up the stack,
            //but we should not mess up the process of adding a quick list item to the cart for it either.
            //We discussed possibly just bumping the end date time out by a minute (since we're calculating it here.
            //But we decieded against that for now.  So we could still have a condition where the end date
            //is earlier than the begin date.
            //Winston Murdockm 03/31/2021.  EMAR-864


            cartOrder.EndDatetime = // duration and duration unit have been set
                                    (cartOrder.Duration != null && cartOrder.DurationUnit != null
                                        // duration unit is not "dose"
                                        ? cartOrder.DurationUnit.DurationInMinutes != 0
                                            // calculate total number of minutes from selected duration
                                            // and duration unit and add to order's begin datetime
                                            ? cartOrder.BeginDatetime.AddMinutes((double)cartOrder.Duration * cartOrder.DurationUnit.DurationInMinutes)
                                            // duration unit is "dose" and there are administrations
                                            : cartOrder.CartOrderAdministrations.Any()
                                                // calculate total number of minutes from order's begin datetime
                                                // until selected (via duration) administration's scheduled start datetime
                                                // and add to order's begin datetime
                                                ? cartOrder.BeginDatetime
                                                    .AddMinutes(cartOrder.CartOrderAdministrations.ToList()
                                                        [(int)cartOrder.Duration > cartOrder.CartOrderAdministrations.Count
                                                            ? cartOrder.CartOrderAdministrations.Count - 1
                                                            : (int)cartOrder.Duration - 1]
                                                        .AdministrationScheduledDatetime.Subtract(cartOrder.BeginDatetime).TotalMinutes)
                                                : (DateTimeOffset?)null
                                        : cartOrder.CartOrderAdministrations.Any()
                                            // calculate total number of minutes from order's begin datetime
                                            // until last administration's scheduled start datetime
                                            // and add to order's begin datetime
                                            ? cartOrder.BeginDatetime.AddMinutes(cartOrder.CartOrderAdministrations.Last().AdministrationScheduledDatetime.Subtract(cartOrder.BeginDatetime).TotalMinutes)
                                            : (DateTimeOffset?)null);


            ////****************************************************
            ////This is an unroll of the huge line above.
            ////We had no idea what is was doing befroe i did this.
            //DateTimeOffset? testingDateTimeOffset;

            //// if duration and duration unit have been set
            //if (cartOrder.Duration != null && cartOrder.DurationUnit != null)
            //{
            //    // if duration unit is not "dose"
            //    if (cartOrder.DurationUnit.DurationInMinutes != 0)
            //    {
            //        // calculate total number of minutes from selected duration
            //        // and duration unit and add to order's begin datetime
            //        testingDateTimeOffset = cartOrder.BeginDatetime.AddMinutes((double)cartOrder.Duration * cartOrder.DurationUnit.DurationInMinutes);
            //    }
            //    else
            //    {
            //        // if duration unit is "dose" and there are administrations
            //        if (cartOrder.CartOrderAdministrations.Any())
            //        {
            //            // calculate total number of minutes from order's begin datetime
            //            // until selected (via duration) administration's scheduled start datetime
            //            // and add to order's begin datetime
            //            testingDateTimeOffset = cartOrder.BeginDatetime
            //                                        .AddMinutes(cartOrder.CartOrderAdministrations.ToList()
            //                                            [(int)cartOrder.Duration > cartOrder.CartOrderAdministrations.Count
            //                                                ? cartOrder.CartOrderAdministrations.Count - 1
            //                                                : (int)cartOrder.Duration - 1]
            //                                            .AdministrationScheduledDatetime.Subtract(cartOrder.BeginDatetime).TotalMinutes);
            //        }
            //        else
            //        {
            //            // null
            //            testingDateTimeOffset = (DateTimeOffset?)null;
            //        } //end if
            //    } //end if
            //}
            //else
            //{
            //    // if there are administrations.
            //    if (cartOrder.CartOrderAdministrations.Any())
            //    {
            //        // calculate total number of minutes from order's begin datetime
            //        // until last administration's scheduled start datetime
            //        // and add to order's begin datetime
            //        testingDateTimeOffset = cartOrder.BeginDatetime.AddMinutes(cartOrder.CartOrderAdministrations.Last().AdministrationScheduledDatetime.Subtract(cartOrder.BeginDatetime).TotalMinutes);
            //    }
            //    else
            //    {
            //        // null
            //        testingDateTimeOffset = (DateTimeOffset?)null;
            //    } //end if
            //} //end if
            ////****************************************************


            //If we have an end date time...
            if (cartOrder.EndDatetime.HasValue)
            {
                //Copy the cart order over to a local variable.
                var order = cartOrder;

                //Sometimes we're hitting a situation where the end time is earlier than the administration's scheduled date time.
                //In that case, the Lambda below returns zero administrations, and the if condition below fails because
                //cartOrder.CartOrderAdministrations.Last() cannot be called on a list with zero members.
                //Suggestion = add one to the end date time only for the comparison without actually changing the value in the object.
                //Original logic here...
                //cartOrder.CartOrderAdministrations =
                //    cartOrder.CartOrderAdministrations
                //        .Where(a =>
                //            a.AdministrationScheduledDatetime == order.EndDatetime
                //            || a.AdministrationScheduledDatetime < order.EndDatetime)
                //        .ToList();

                //Don't add one minute.
                //Use the original handling tsince the UI and DB have handled this.
                //Add one minute to the end date time so that the comparison below never
                //hits the status where the administration's scheduled date time is
                //later than then cart order's end date time.
                //We are not changing the actual end date time value for the cart order.
                //We are merely using this temp variable for the comparison below.
                DateTimeOffset tempEndDateTime;
                tempEndDateTime = (DateTimeOffset)order.EndDatetime;
                tempEndDateTime = tempEndDateTime.AddMinutes(1);

                cartOrder.CartOrderAdministrations =
                    cartOrder.CartOrderAdministrations
                        .Where(a =>
                            a.AdministrationScheduledDatetime == tempEndDateTime
                            || a.AdministrationScheduledDatetime < tempEndDateTime)
                        .ToList();

                //Adding a try/catch so that we can handle the exception silently without
                //stopping the whole process.
                try
                {
                    //If we ahve any order administrations in the list (i.e. any that are earlier than the end date time).
                    //This line should prevent the try catch from being necessary.
                    //But I'd rather leave the try catch in just in case.
                    if (cartOrder.CartOrderAdministrations.Any())
                    {
                        //If the last order administration is point in time.
                        if (!cartOrder.CartOrderAdministrations.Last().PointInTime)
                        {
                            //We're guaranteed to have an end date time here since there's an is null check on it above.
                            //In the case where it's null, we just won't set the scheduled stop date time on the last order.
                            //Set the last administration's scheudled stop time to the end time of the order.
                            cartOrder.CartOrderAdministrations.Last().StopScheduledDatetime = cartOrder.EndDatetime;
                        } //end if (Point in Time?)
                    } //end if (any administrations?)
                }
                catch (Exception ex)
                {
                    //Swallow the error and move on.
                    //No need to hold up the entire process of adding a quick list item to the cart for this.
                } //end try/catch
            } //end if (end date time is not null?)
        }

        #region Drug Interactions & Allergy Reactions
        public IEnumerable<MedicationInteractionReaction> CheckInteractionsReactions(in int userId, List<MedicationModel> medicationList, long patientId, bool checkAgainstCartOrders = true)
        {
            if (medicationList.Count < 1)
            {
                return null;
            }

            var medications =
                MedicationManager.AddInteractionsAndReactionsToMedications(
                    userId,
                    medicationList[0].SiteId > 0
                        ? medicationList[0].SiteId
                        : _patientRepository.GetSiteIdForPatient(patientId),
                    patientId,
                    medicationList,
                    _orderRepository,
                    _cartOrderRepository,
                    _homeMedicationRepository,
                    _patientRepository,
                    _optionRepository,
                    checkAgainstCartOrders
                )
                .Select(OrderMapper.MedicationInteractionsReactions);

            return medications;
        }

        public void UpdatePatientOrderInteractionsAndReactions(long patientId)
        {
            var siteId = _patientRepository.GetSiteIdForPatient(patientId);
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == patientId);
            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == patientId);

            foreach (var order in orders)
            {
                var items = new List<MedicationModel> {
                    OrderMapper.MapOrderItemToModel(EmarOrderType.PatientOrder, order, order.PatientId, order.AddUserId, null)
                };

                UpdateOrderInteractionsAndReactions(siteId, patientId, order.Id, order.MedicationId, order.AddUserId, EmarOrderType.PatientOrder, items);
            }
            foreach (var order in cartOrders)
            {
                var items = new List<MedicationModel> {
                    OrderMapper.MapOrderItemToModel(EmarOrderType.PatientCartOrder, order, order.PatientId, order.UserId, null)
                };

                UpdateOrderInteractionsAndReactions(siteId, patientId, order.Id, order.MedicationId, order.UserId, EmarOrderType.PatientCartOrder, items);
            }
        }

        private void UpdateOrderInteractionsAndReactions(int siteId, long patientId, long orderId, int medicationId, int userId, EmarOrderType orderType, List<MedicationModel> items)
        {
            items[0].SiteId = siteId;
            items[0].Medication = MedicationMapper.MapMedication(_medicationRepository.GetMedication(medicationId), null);

            var interactionsReactions = CheckInteractionsReactions(userId, items, patientId, orderType == EmarOrderType.PatientCartOrder);

            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, orderId, orderType, false);
        }
        #endregion

        #region Scheduler Support Methods
        public SchedulerOptionsDto GetSchedulerSetupData(int siteId, string brandName)
        {
            var medications = _orderRepository
                .GetSchedulerSetupData(siteId, brandName)
                .ToList();

            if (!medications.Any() || medications.All(m => m == null))
            {
                return null;
            }

            var antimicrobialRequiredIndicators = _orderRepository.GetAntimicrobialRequiredIndicators(siteId, medications);

            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            var orderInstructions = new List<OrderInstruction>();

            var sharedSiteId = codeShareSites
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.OrderInstruction)?
                .SharedSiteId;

            if (sharedSiteId != null)
            {
                orderInstructions.AddRange(_orderRepository.GetOrderInstructions(sharedSiteId.Value).ToList());
            }

            return OrderMapper.MapSchedulerSetupData(brandName, medications, antimicrobialRequiredIndicators, null, orderInstructions, codeShareSites);
        }

        public SchedulerOptionsDto GetSchedulerSetupData(int siteId, EmarOrderType itemType, int itemId)
        {
            var medications = _orderRepository
                .GetSchedulerSetupData(siteId, itemType, itemId)
                .ToList();

            if (!medications.Any() || medications.All(m => m == null))
            {
                return null;
            }

            var antimicrobialRequiredIndicators = _orderRepository.GetAntimicrobialRequiredIndicators(siteId, medications);
            var administrations = _orderRepository.GetSchedulerAdministrations(siteId, itemType, itemId, _siteRepository.DateTimeOffsetNow(siteId), null);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            var orderInstructions = new List<OrderInstruction>();

            var sharedSiteId = codeShareSites
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.OrderInstruction)?
                .SharedSiteId;

            if (sharedSiteId != null)
            {
                orderInstructions.AddRange(_orderRepository.GetOrderInstructions(sharedSiteId.Value).ToList());
            }

            return OrderMapper.MapSchedulerSetupData(medications.FirstOrDefault()?.DisplayName, medications, antimicrobialRequiredIndicators, administrations, orderInstructions, codeShareSites);
        }

        public IEnumerable<FrequencyScheduleDto> GetFrequencies(int siteId)
        {
            var sharedSiteId = _orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.FrequencySchedule)?
                .SharedSiteId;

            if (sharedSiteId == null)
            {
                return null;
            }

            return _orderRepository.GetScheduleFrequencies(sharedSiteId.Value)
                .Select(OrderMapper.MapFrequencySchedule);
        }

        public IEnumerable<MedicationRouteDto> GetRoutes(int siteId)
        {
            var sharedSiteId = _orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationRoute)?
                .SharedSiteId;

            if (sharedSiteId == null)
            {
                return null;
            }

            //Order by priority and then by name.
            //Winston Murdock, 02/25/2021.  EMAR-779
            return _orderRepository.GetRoutes(siteId)
                .Select(OrderMapper.MapMedicationRoute).OrderBy(a => a.Priority).ThenBy(b => b.RouteName);
        }

        public IEnumerable<MedicationUnitDto> GetUnits(int siteId)
        {
            var sharedSiteId = _orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            if (sharedSiteId == null)
            {
                return null;
            }

            //Order by priority and then by name.
            //Winston Murdock, 02/25/2021.  EMAR-779
            return _orderRepository.GetUnits(sharedSiteId.Value)
                .Select(OrderMapper.MapMedicationUnit).OrderBy(a => a.Priority).ThenBy(b => b.UnitName);
        }

        public IEnumerable<FrequencyScheduleAdministrationDto> GetNewAdministrations(int siteId, int frequencyId, DateTimeOffset? start, DateTimeOffset? stop)
        {
            return _orderRepository.GetNewAdministrations(siteId, frequencyId, start ?? _siteRepository.DateTimeOffsetNow(siteId), stop)
                .Select(OrderMapper.MapFrequencyScheduleAdministration);
        }
        #endregion Scheduler Support Methods

        public IEnumerable<DurationUnitDto> GetDurationUnits()
        {
            return _orderRepository.GetDurationUnits()
                .Select(OrderMapper.MapDurationUnit);
        }
    }
}
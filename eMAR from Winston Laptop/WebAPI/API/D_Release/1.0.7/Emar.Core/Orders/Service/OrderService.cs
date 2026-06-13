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

            //This guy calls the GetHashCode method.
            //Theoretically, my changes inside it will fix the situation.
            //I'll test after lunch.
            //Winston Murdock, 05/02/2022.
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

            //Get all of the orders for this patient.
            //Then we'll pass this list down rather than retrieving it each time.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            //Get all of the cartOrders for this patient / user(we are recalculating for each of them).
            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //There are no combo meds in the quick list.  So we don't have to worry about interactions between a medication and itself.

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

            //var interactionsReactions = CheckInteractionsReactions(resource.UserId, itemList, resource.PatientId);
            var interactionsReactions = CheckInteractionsReactions
                (
                    resource.UserId,
                    itemList,
                    resource.PatientId,
                    true,
                    null,
                    orders,
                    cartOrders,
                    patientAllergies,
                    patientHomeMedications
                );

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

        public CartOrderDto CopyQuickListItemToCart(in int quickListItemId, BaseLinkResource resource, int? duration = null, int? durationUnitId = null)
        {
            var quickListItem = _orderRepository.GetUserQuickListItem(quickListItemId);

            if (quickListItem == null)
            {
                return null;
            }

            //If the duration and duration_unit_id parameters are null, then attempt to grab those values from quickListItem.
            //If they have a value, then use them.
            //Winston Murdock, 08/24/2021.  EMAR-1162
            if (!duration.HasValue)
            {
                //Grab the duration from the object.
                //If it's null, then fine.
                //If it's not null, then it will be whatever is in the DB.
                duration = quickListItem.Duration;
            } //end if

            if (!durationUnitId.HasValue)
            {
                //Grab the duration unit id from the object.
                //IF it's null, then fine.
                //If it's not null, then it will be whatever is in the DB.
                durationUnitId = quickListItem.DurationUnitId;
            } //end if

            var cartOrder = OrderMapper.MapUserQuickListItemToPatientCartOrder(resource.UserId, (int)resource.PatientId, quickListItem);
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrder.PatientId);
            var admins = _orderRepository.GetNewAdministrations(siteId, cartOrder.FrequencyScheduleId ?? -1,
                _siteRepository.DateTimeOffsetNow(siteId), null, duration, durationUnitId);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            AddEndDatetime_FixNoOfAdministrations(ref cartOrder);

            var newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            //Get the list of orders and the list of cart orders.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //Remove this cart order from the list of cart orders so that we don't do interaction checking against itself.
            var cartOrdersMinusThisOne = cartOrders.Where(co => co.Id != newCartOrder.Id);

            //Also remove any cart orders with the same medication id as this one.
            //That way we don't check a GI Cocktail against itself.
            cartOrdersMinusThisOne = cartOrders.Where(co => co.MedicationId != newCartOrder.MedicationId);

            //Also remove any patient orders with the same medication id as this one.
            //That way we don't check a GI Cocktail against itself.
            var patientOrdersMinusThisOne = orders.Where(o => o.MedicationId != newCartOrder.MedicationId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

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
                resource.PatientId,
                true,
                null,
                patientOrdersMinusThisOne,
                cartOrdersMinusThisOne,
                patientAllergies,
                patientHomeMedications);

            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id);

            return CartOrderMapper.MapCartOrder(newCartOrder, drugDbVendor, codeShareSites, resource);
        }

        public bool DeleteQuickListItem(int quickListItemId)
        {
            return _orderRepository.DeleteQuickListItem(quickListItemId);
        } //end DeleteQuickListItem

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

        public IEnumerable<DepartmentPreferredItemDto> GetDepartmentPreferredListByTab(string departmentCode, BaseLinkResource resource, string tabName)
        {
            var tabItems = _orderRepository.GetDepartmentPreferredListByTab(tabName, resource, departmentCode).ToList();

            if (!tabItems.Any())
            {
                return null;
            }

            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            var orderedTabItems = tabItems
                .Select(item => OrderMapper.MapDepartmentPreferredListItem(item, resource, codeShareSites))
                .OrderBy(i => i.Medication.DisplayName)
                .ToList();

            orderedTabItems = AddInteractionsReactions(orderedTabItems.ToList(), resource);

            return orderedTabItems;
        } //end GetDepartmentPreferredListByTab
        
        public DepartmentPreferredFrameworkDto GetInitialDepartmentPreferredList(string departmentCode, BaseLinkResource resource)
        {
            var tabList = _orderRepository.GetDepartmentPreferredListTabs(departmentCode, resource).OrderBy(i => i.Key).ToList();

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

            var items = _orderRepository.GetDepartmentPreferredListByTab(tabList[0].Key, resource, departmentCode).ToList();

            var firstTabContents = items
                .Select(item => OrderMapper.MapDepartmentPreferredListItem(item, resource, codeShareSites))
                .OrderBy(i => i.Medication.DisplayName)
                .ToList();

            firstTabContents = AddInteractionsReactions(firstTabContents.ToList(), resource);

            return new DepartmentPreferredFrameworkDto(firstTabContents, tabList, resource.LinkGetDepartmentPreferredListTab);
        } //end GetInitialDepartmentPreferredList

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


            //Get all of the orders for this patient.
            //Then we'll pass this list down rather than retrieving it each time.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            //Get all of the cartOrders for this patient / user(we are recalculating for each of them).
            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //There are no combo meds in the department prefered list.  So we don't have to worry about interactions between a medication and itself.

            //var interactionsReactions = CheckInteractionsReactions(resource.UserId, itemList, resource.PatientId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

            var interactionsReactions = CheckInteractionsReactions
                (
                    resource.UserId,
                    itemList,
                    resource.PatientId,
                    true,
                    null,
                    orders,
                    cartOrders,
                    patientAllergies,
                    patientHomeMedications
                );

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
                _siteRepository.DateTimeOffsetNow(siteId), null, null, null);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            AddEndDatetime_FixNoOfAdministrations(ref cartOrder);

            var newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            //Get the list of orders and the list of cart orders.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //Remove this cart order from the list of cart orders so that we don't do interaction checking against itself.
            var cartOrdersMinusThisOne = cartOrders.Where(co => co.Id != newCartOrder.Id);

            //Also remove any cart orders with the same medication id as this one.
            //That way we don't check a GI Cocktail against itself.
            cartOrdersMinusThisOne = cartOrders.Where(co => co.MedicationId != newCartOrder.MedicationId);

            //Also remove any patient orders with the same medication id as this one.
            //That way we don't check a GI Cocktail against itself.
            var patientOrdersMinusThisOne = orders.Where(o => o.MedicationId != newCartOrder.MedicationId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

            //Pass in the lists of orders and cart orders so that we don't have to grab them later.
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
                resource.PatientId,
                true,
                null,
                patientOrdersMinusThisOne,
                cartOrdersMinusThisOne,
                patientAllergies,
                patientHomeMedications);

            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id);

            return CartOrderMapper.MapCartOrder(newCartOrder, drugDbVendor, codeShareSites, resource);
        }
        #endregion Department Preferred List Services

        #region Group Remembered Order Services
        public GroupsRememberedOrdersDto GetGroupsRememberedOrdersList(string departmentCode, BaseLinkResource resource)
        {
            //This was getting the group list items.
            //Then it got the code share sites.
            //Then it passed the code share sites into the mapper.
            //We do want to pass the code share sites into the mapper (so we can get the correct routes, units, etc...).
            //But we also need to use code share sites to get the group list items for the site we're pulling from.
            //So I'm moving the logic to get the code share sites to the top of this method.
            //And I'm setting the siteId in the resource to the shared site id.
            //Winston Murdock, 05/05/2021.  EMAR-812.
            
            //Get the list of code share sites for this site.
            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);
            
            //Get the id of the site we're pulling the group list from.
            //The "entity" for this is "services."
            var groupSiteId = codeShareSites
                                .FirstOrDefault(c =>
                                    c.Entity == OrderRepository.CodeShareEntity.Service)?
                                .SharedSiteId;

            //If we don't have a site id, use the id of the site the user is logged in to.
            if (groupSiteId == null)
            {
                groupSiteId = resource.SiteId;
            }

            //Edit the site id in the resource to be the id we're pulling from for groups.
            resource.SiteId = groupSiteId.Value;

            //Now that we've set the site id corretly, get the group list items.
            var items = _orderRepository.GetGroupRememberedOrderItems(departmentCode, resource)
                .ToList();

            //If we don't have any group list items return null.
            if (!items.Any()) return null;
            
            var itemList = items
                .Select(item => OrderMapper.MapGroupListItem(item, resource, codeShareSites))
                .ToList();
            
            itemList = AddInteractionsReactions(itemList, resource);
            
            var ret = new GroupsRememberedOrdersDto();

            foreach (var groupName in items.GroupBy(i => i.GroupName).Select(i => i.Key).OrderBy(i => i))
            {
                //Added the order by to order the list of medications for this group alphabetically.
                //Winston Murdock, 10/18/2021.  PC-26672
                var groupList = itemList
                    .Where(i => i.GroupName == groupName)
                    .ToList().OrderBy(i => i.Medication.DisplayName);

                ret.Groups.Add(new RememberedGroupDto
                {
                    GroupName = groupName,
                    Orders = groupList
                });
            }

            return ret;
        }

        public List<string> GetGroupNames(int siteId, string? departmentCode = null)
        {
            //Get the names of each group for the specified siteId and departmentCode.
            //If the departmentCode is null, then only filter by siteId.
            //Winston Murdock, 07/18/2022.

            //Get the list of code share sites for this site.
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            //Get the id of the site we're pulling the group list from.
            //The "entity" for this is "services."
            var groupSiteId = codeShareSites
                                .FirstOrDefault(c =>
                                    c.Entity == OrderRepository.CodeShareEntity.Service)?
                                .SharedSiteId;

            //If we don't have a site id, use the id of the site the user is logged in to.
            if (groupSiteId == null)
            {
                groupSiteId = siteId;
            }

            //Now that we've set the site id corretly, get the group list items.
            //This should return a dictionary<groupName, departmentCode>
            var ret = _orderRepository.GetGroupNames((int)groupSiteId, departmentCode).ToList();

            //Order the list alphabetically.
            ret = ret.OrderBy(x => x).ToList();

            //Return
            return ret;
        } //end GetGroupNames

        public GroupsRememberedOrdersDto GetGroupItemsByGroupName(string departmentCode, BaseLinkResource resource, string groupNameForFilter)
        {
            //This is a copy of GetGroupsRememberedOrdersList.  The original gets all of the group items for the site/department.
            //This one gets all of the group items in the passed in group name.
            //We're moving to a two-step process.
            //1) Call GetGroupNames to get the list of group names.
            //2) Call GetGroupItemsByGroupname for the selected group to get the items inside that group.
            //In the UI, clicking the name of a group will fire off item 2 from above.
            //This will require UI changes, but I've left the existing function here
            //so that the UI will continue to work until they make the necessary changes.
            //Also, I'm going to incorporate my changes that do the interaction checking for
            //individual medications inside combo meds.
            //I did have them as part of the main method, but it was rather slow.
            //And I had to shelve those to work on other things.
            //And Colin reminded me that Hsi-An and I had wanted to move to this two-step approach in early 2021.
            //Winston Murdock, 07/19/2022.

            //This was getting the group list items.
            //Then it got the code share sites.
            //Then it passed the code share sites into the mapper.
            //We do want to pass the code share sites into the mapper (so we can get the correct routes, units, etc...).
            //But we also need to use code share sites to get the group list items for the site we're pulling from.
            //So I'm moving the logic to get the code share sites to the top of this method.
            //And I'm setting the siteId in the resource to the shared site id.
            //Winston Murdock, 05/05/2021.  EMAR-812.

            //Get the list of code share sites for this site.
            var codeShareSites = _orderRepository.GetCodeShareSites(resource.SiteId);

            //Get the id of the site we're pulling the group list from.
            //The "entity" for this is "services."
            var groupSiteId = codeShareSites
                                .FirstOrDefault(c =>
                                    c.Entity == OrderRepository.CodeShareEntity.Service)?
                                .SharedSiteId;

            //If we don't have a site id, use the id of the site the user is logged in to.
            if (groupSiteId == null)
            {
                groupSiteId = resource.SiteId;
            }

            //Edit the site id in the resource to be the id we're pulling from for groups.
            resource.SiteId = groupSiteId.Value;

            //Now that we've set the site id corretly, get the group list items.
            //var items = _orderRepository.GetGroupRememberedOrderItems(departmentCode, resource).ToList();
            var items = _orderRepository.GetGroupItemsByGroupName(departmentCode, resource, groupNameForFilter).ToList();

            //If we don't have any group list items return null.
            if (!items.Any()) return null;

            //This is the oringinal filtering logic that doesn't account for combo meds in group items.
            //var itemList = items
            //    .Select(item => OrderMapper.MapGroupListItem(item, resource, codeShareSites))
            //    .ToList();
            //
            //itemList = AddInteractionsReactions(itemList, resource);

            //For non-combo meds, we need to pass in the list of Dto objects.
            //For combo meds, we need to get the actual medications for each detail item
            //and then do interaction checking for each of them.
            //1) Filter the list of entities into a new variable to only have the non combo meds.
            //2) Filter the list of entities into a new variable to only have the combo meds.
            //3) Map the non combo med entities to Dtos.
            //4) Map the combo med entities to Dtos.
            //5) Grab the interactions/reactions for non combo meds and map them to the appropriate item.
            //6) Grab the interactions/reactions for non combo meds and map them to the appropriate item.
            //7) The interaction/reaction stuff will call the method to combine all of the
            //      interactions/reactions for the medication details inside of each combo med to be
            //      listed as interactions/reactions for the combo med.
            //8) Add the list of non combo med Dtos to the return variable.
            //9) Add the list of combo med Dtos  to the return variable.
            //10) Return.
            //Winston Murdock, 05/23/2022.  PC-27238.

            //1) Filter the list of entities into a new variable to only have the non combo meds.
            var nonComboMeds = items.Where(x => x.Medication.DrugId != "COMBO").ToList();

            //2) Filter the list of entities into a new variable to only have the combo meds.
            var comboMeds = items.Where(x => x.Medication.DrugId == "COMBO").ToList();

            //3) Map the non combo meds to Dtos.
            var nonComboMedsDto = nonComboMeds
                .Select(item => OrderMapper.MapGroupListItem(item, resource, codeShareSites))
                .ToList();

            //4) Map the combo meds to Dtos.
            var comboMedsDto = comboMeds
                .Select(item => OrderMapper.MapGroupListItem(item, resource, codeShareSites))
                .ToList();

            //5) Grab the interactions/reactions for non combo meds and map them to the appropriate item.
            // Only do this if we have any non combo meds in this group.
            if (nonComboMedsDto.Any())
            {
                nonComboMedsDto = AddInteractionsReactions(nonComboMedsDto, resource);
            } //end if

            //6) Grab the interactions/reactions for non combo meds and map them to the appropriate item.
            // Only do this if we have any combo meds in this group.
            if (comboMedsDto.Any())
            {
                //7) The interaction/reaction stuff will call the method to combine all of the
                //      interactions/reactions for the medication details inside of each combo med to be
                //      listed as interactions/reactions for the combo med.
                comboMedsDto = AddInteractionsReactionsCheckForComboMeds(comboMedsDto, resource);
            } //end if (Do we have any combo meds?)

            //8) Add the list of non combo med Dtos to the return variable.
            var itemList = nonComboMedsDto;

            //9) Add the list of combo med Dtos  to the return variable.
            itemList.AddRange(comboMedsDto);

            //This groups the return values by group name then by individual item name.
            //At this point itemList is both the non combo med Dtos and the combo med Dtos.
            var ret = new GroupsRememberedOrdersDto();

            foreach (var groupName in items.GroupBy(i => i.GroupName).Select(i => i.Key).OrderBy(i => i))
            {
                //Added the order by to order the list of medications for this group alphabetically.
                //Winston Murdock, 10/18/2021.  PC-26672
                var groupList = itemList
                    .Where(i => i.GroupName == groupName)
                    .ToList().OrderBy(i => i.Medication.DisplayName);

                ret.Groups.Add(new RememberedGroupDto
                {
                    //If the patient has an order that interacts with multiple of the orders
                    //in this combo med, or if the patient has an allergy to multiple of the
                    //orders within this combo med, then that interaction/reaction will show
                    //multiple times.
                    //Winston Murdock, 07/20/2022.
                    GroupName = groupName,
                    Orders = groupList
                });
            } //end foreach

            //10) Return.
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

            //var interactionsReactions = CheckInteractionsReactions(resource.UserId, itemList, resource.PatientId);

            //Get the list of orders that aren't cancelled or deleted
            //and the list of cart orders for this user/patient.
            //Then pass them along rather than having other code pull them for each loop.
            //Then we'll pass this list down rather than retrieving it each time.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

            var interactionsReactions = CheckInteractionsReactions
                (
                    resource.UserId,
                    itemList,
                    resource.PatientId,
                    true,
                    null,
                    orders,
                    cartOrders,
                    patientAllergies,
                    patientHomeMedications
                );

            //Loop through each interaction or reaction and match them to the correct group order.
            //We'll also need combo med specific logic in here.
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

        private List<GroupListItemDto> AddInteractionsReactionsCheckForComboMeds(List<GroupListItemDto> comboMeds, BaseLinkResource resource)
        {
            var ret = new List<GroupListItemDto>();

            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(resource.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            //var itemList = comboMeds
            //    .Select(item => OrderMapper.MapOrderItemDtoToModel(EmarOrderType.GroupRememberedOrder, item, resource.PatientId, resource.UserId))
            //    .ToList();

            //Get the list of orders that aren't cancelled or deleted
            //and the list of cart orders for this user/patient.
            //Then pass them along rather than having other code pull them for each loop.
            //Then we'll pass this list down rather than retrieving it each time.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

            //Make a copy of the method we use in MedicationService and then call that here.
            foreach (GroupListItemDto comboMed in comboMeds)
            {
                //var comboMed = comboMeds[7];

                //Get the medications for each of the medication details inside this combo med.
                //Then run the interaction checking for each of those medications.
                //Then combin those interactions for the medication details into one list.
                //Lastly, set the interactions and reactions for this one group item to that combined list.

                var interactionsReactionsComboMed = new List<MedicationInteractionReaction>();

                foreach (var medDetail in comboMed.Medication.MedicationDetails)
                {
                    //Get the actual Medication for this one (not the combo med).
                    //It will match on drug_id.
                    var medication = _medicationRepository.GetMedicationByDrugId(medDetail.DrugId);

                    //Go get the interactions and reactions for this one medication and add them to the list.
                    interactionsReactionsComboMed.AddRange
                    (
                        CheckInteractionsReactions
                        (
                            resource.UserId,
                            new List<MedicationModel>
                            {
                                    OrderMapper.MapOrderItemToModel(EmarOrderType.MedicationItem, medication, resource.PatientId, resource.UserId, codeShareSiteMedicationUnit)
                            },
                            resource.PatientId,
                            false,
                            null,
                            orders,
                            cartOrders,
                            patientAllergies,
                            patientHomeMedications
                        )
                        .ToList()
                    );
                } //end foreach detail item inside the combo med.

                //Now that we've got all of the interactions and reactions, combine them into one set.
                //Need to write a version of this (in Medication Service is fine) that takes in a GroupListItemDto instead of a GroupList.
                //Then do the same stuff.
                MedicationInteractionReaction oneInteractionReaction = CompressComboMedDtoInteractionsToOneEntry(comboMed, resource.UserId, interactionsReactionsComboMed);

                //If multiple medications in the combo med interact with the same existing medication
                //(or hit the same reaction to an allergy), they will be listed multiple times.
                //From what I saw on interactions, the only difference is DrugInteractions.InteractionDrug1
                //(the item in the combo med that we interact with or react to).
                //ToDo: figure out a way to strip out the duplicates.
                //Winston Murdock, 07/20/2022.  PC-27238
                foreach (var interaction in oneInteractionReaction.Interactions)
                {
                    comboMed.AddOrderInteraction(MedicationMapper.MapOrderInteraction(MedicationMapper.MapInteractionDictionaryToMedicationInteraction(interaction), drugDbVendor, resource, codeShareSiteMedicationUnit));
                } //end foreach interaction

                foreach (var reaction in oneInteractionReaction.Reactions)
                {
                    comboMed.AddAllergyReaction(MedicationMapper.MapReactionDictionaryToAllergyReactionViewDto(reaction, oneInteractionReaction));
                } //end foreach reaction.

                //Add this combo med Dto to the retun variable.
                ret.Add(comboMed);
            } //end foreach combo med

            //At this point, we've done the normal interaction/reaction checking for the group orders that aren't combo meds.
            //And we've done the combo-med specific logic for each of the group orders that are combo meds.
            //Return.
            return ret;
        } //end AddInteractionsReactionsCheckForComboMeds

        private IEnumerable<MedicationInteractionReaction> AddInteractionsReactionsCheckForComboMedsReturnList
        (
            Medication comboMed,
            BaseLinkResource resource,
            long? cartOrderId = null,
            long? orderId = null,
            IEnumerable<PatientAllergy>? patientAllergies = null,
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null
        )
        {
            //This is similar to the above method.
            //But instead of taking in a list of group items (that are combo meds)
            //it takes in only one combo med.
            //And it returns the interactions/reactions rather than setting them as
            //children of the combo med group item.
            //Winston Murdock, 08/09/2022.  PC-27326
            var interactionsReactionsComboMed = new List<MedicationInteractionReaction>();

            var drugDbVendor = _optionRepository.GetOption(resource.SiteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(resource.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            //Get the list of orders that aren't cancelled or deleted
            //and the list of cart orders for this user/patient.
            //Then pass them along rather than having other code pull them for each loop.
            //Then we'll pass this list down rather than retrieving it each time.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //We don't want to check this guy against himself.
            //But we don't know if this guy is a patient order or a cart order.
            //Thus, we'll need to filter out any orders or cart orders from the
            //lists that have the same medication id as this guy.
            //comboMed is a Medication entity.
            //Winston Murdock, 08/17/2022.  PC-24742.  
            //TODO: Test this.  This guy is called by adding a group order straight to the cart.
            //But it's called by other places too.  Thus, our needing to filter this medication
            //out of both the orders and cart orders lists.
            //var ordersMinusThisOne = orders.Where(o => o.Medication.Id != comboMed.Id);
            //var cartOrdersMinusThisOne = cartOrders.Where(co => co.Medication.Id != comboMed.Id);

            if (cartOrderId.HasValue)
            {
                cartOrders = cartOrders.Where(co => co.Id != cartOrderId);
            } //end if

            //Remove any cart orders with the same medicationId as this order.
            //That way we don't try to check a combo med against another order for the same combo med.
            //On 57c, we have a combo med that containts two orders that interact with each other.
            //And we don't want to show GI Cocktail interacting with GI Cocktail.
            //Winston Murdock, 09/02/2022.  PC-27472
            cartOrders = cartOrders.Where(co => co.MedicationId != comboMed.Id);

            if (orderId.HasValue)
            {
                //Possibly need to filter out any orders with the same medication id as this order.
                //That way we would not check a combo med against itself should the user have
                //ordered the same combo med order twice.
                //The easiest way to do that would be to pass in the order's medication id as
                //a parameter and then use that if we have it.  If we don't have it, then we would
                //fall back to using the order's ID.
                //We wouldn't want to calculate Tylenol against Tylenol or GI Cocktail against GI Cocktail.
                //I would want to test, of course.  Already have an order for this med (regular or combo)
                //then place a new order.  Confirm we don't get any funky interaction stuff.
                orders = orders.Where(o => o.Id != orderId);
            } //end if

            //Remove any orders with the same medicationId as this order.
            //That way we don't try to check a combo med against another order for the same combo med.
            //On 57c, we have a combo med that containts two orders that interact with each other.
            //And we don't want to show GI Cocktail interacting with GI Cocktail.
            //Winston Murdock, 09/02/2022.  PC-27472
            orders = orders.Where(o => o.MedicationId != comboMed.Id);

            //Get the medications for each of the medication details inside this combo med.
            //Then run the interaction checking for each of those medications.
            //Then combin those interactions for the medication details into one list.
            //Lastly, set the interactions and reactions for this one group item to that combined list.

            foreach (var medDetail in comboMed.MedicationDetails)
            {
                //Get the actual Medication for this one (not the combo med).
                //It will match on drug_id.
                var medication = _medicationRepository.GetMedicationByDrugId(medDetail.DrugId);

                //Go get the interactions and reactions for this one medication and add them to the list.
                interactionsReactionsComboMed.AddRange
                (
                    CheckInteractionsReactions
                    (
                        resource.UserId,
                        new List<MedicationModel>
                        {
                                    OrderMapper.MapOrderItemToModel(EmarOrderType.MedicationItem, medication, resource.PatientId, resource.UserId, codeShareSiteMedicationUnit)
                        },
                        resource.PatientId,
                        false,
                        null,
                        orders,
                        cartOrders,
                        patientAllergies,
                        patientHomeMedications
                    )
                    .ToList()
                );
            } //end foreach detail item inside the combo med.

            //At this point, we've done the normal interaction/reaction checking for the group orders that aren't combo meds.
            //And we've done the combo-med specific logic for each of the group orders that are combo meds.
            //Return.
            return interactionsReactionsComboMed;
        } //end AddInteractionsReactionsCheckForComboMedsReturnList

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
                _siteRepository.DateTimeOffsetNow(siteId), null, null, null);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            AddEndDatetime_FixNoOfAdministrations(ref cartOrder);

            var newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            //We should get the list of orders and cart orders here.
            //Then we can pass them down so that interaction checking is faster.
            //The current parameter list stops at patient id.
            //We need to add check against cart orders (true), new orders (null),
            //existing orders (calculated above), cart orders (calculated above),
            //and delete existing (false)
            //Winston Murdock, 08/16/2022.  PC-27472

            //Per Romel, there's no way that a combo med group item can have a frequency.
            //There is no concept of frequency or repeating in PulseCheck.
            //I was only able to add one internally by manually editing the DB.
            //Thus, we don't need to combo med specific logic here.
            //I'll leave the logic but just commented out in case that ever changes in the future.
            //Winston Murdock, 09/01/2022.  
            ////Need to handle a combo med differently than a normal order.
            //if (cartOrder.Medication.DrugId == "COMBO")
            //{
            //    //Now get the interactions/reactions for the medications inside this combo med.
            //    var interactionsReactions = AddInteractionsReactionsCheckForComboMedsReturnList(cartOrder.Medication, resource, cartOrder.Id);

            //    _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, cartOrder.Id, EmarOrderType.PatientCartOrder, false, true);
            //}
            //else
            //{
            //Get the list of orders and the list of cart orders.
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == resource.PatientId);

            orders = orders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == resource.PatientId && order.UserId == resource.UserId);

            //Remove this cart order from the list of cart orders so that we don't do interaction checking against itself.
            var cartOrdersMinusThisOne = cartOrders.Where(co => co.Id != newCartOrder.Id);

            //Also remove any cart orders with the same medication id as this one.
            //That way we don't check a GI Cocktail against itself.
            cartOrdersMinusThisOne = cartOrders.Where(co => co.MedicationId != newCartOrder.MedicationId);

            //Also remove any patient orders with the same medication id as this one.
            //That way we don't check a GI Cocktail against itself.
            var patientOrdersMinusThisOne = orders.Where(o => o.MedicationId != newCartOrder.MedicationId);

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(resource.PatientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == resource.PatientId && a.IsActive);

            //Add the lists of orders to this call.
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
                resource.PatientId,
                true,
                null,
                patientOrdersMinusThisOne,
                cartOrdersMinusThisOne,
                patientAllergies,
                patientHomeMedications);

            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            //} //end if

            //Do we need to get the new cart order here?
            //The call to add new cart order should be returning it.
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id);

            //Since we want to load the groups tab and then show the pathway for this group order,
            //pass those along as parameters to the Mapper.
            //Winston Murdock, 09/23/2022.  PC-27538
            return CartOrderMapper.MapCartOrder(newCartOrder, drugDbVendor, codeShareSites, resource, "groups", groupListItem.GroupName);
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

            //Do this big line twice.
            //Once to calculate the scheduled stop time for the last administration.
            //The second time to calculate the order's end date time if we need to calculate it.
            //Winston Murdock, 02/09/2022.  PC-26986

            //Calculate the scheduled stop time for the last administration
            var lastAdministrationScheduledStopTime = // duration and duration unit have been set
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

            //Calculate the end datetime for the order.
            //If the user selected one in the UI, then we don't need to calculate it and update the value.
            //If the user did not select one but they did select a duration (x doses, minutes, hours, days, etc...),
            //then calculate it based on how long the duration is.
            //If the user did not select one and did not select a duration, then
            //we don't want to have an end time listed.
            //Winston Murdock, 02/09/2022.  PC-26986

            //We only want to calculate and set the EndDateTime if we don't have a value for it and if we do have a duration.
            //If we do have an EndDatetime, then don't change it.
            //If we don't have an EndDateTime and we don't have a duration, then it's appropriate for us not to have an EndDateTime.
            //Winston Murdock, 02/16/2022.  PC-27021
            //if (!cartOrder.EndDatetime.HasValue)
            if ((!cartOrder.EndDatetime.HasValue) && (cartOrder.Duration != null) && (cartOrder.DurationUnit != null))
            {
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
            } //end if


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
            if (lastAdministrationScheduledStopTime.HasValue)
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
                //Use the original handling since the UI and DB have handled this.
                //Add one minute to the end date time so that the comparison below never
                //hits the status where the administration's scheduled date time is
                //later than then cart order's end date time.
                //We are not changing the actual end date time value for the cart order.
                //We are merely using this temp variable for the comparison below.
                DateTimeOffset tempEndDateTime;
                tempEndDateTime = (DateTimeOffset)lastAdministrationScheduledStopTime;
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
                            cartOrder.CartOrderAdministrations.Last().StopScheduledDatetime = lastAdministrationScheduledStopTime;
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
        public IEnumerable<MedicationInteractionReaction> CheckInteractionsReactions(in int userId, List<MedicationModel> medicationList,
            long patientId, bool checkAgainstCartOrders = true, IEnumerable<PatientOrder>? newOrders = null,
            IEnumerable<PatientOrder>? existingOrders = null, IEnumerable<PatientCartOrder>? cartOrders = null,
            IEnumerable<PatientAllergy>? patientAllergies = null, IEnumerable<PatientHomeMedication>? patientHomeMedications = null)
        {
            //Added newOrders, existingOrders, and cartOrders as optional parameters.
            //We need them to speed up the cart checkout process, specifically where
            //we recalculate interactions and reactions for all orders and cart orders.
            //Winston Murdock, 03/14/2022.
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
                    checkAgainstCartOrders,
                    newOrders,
                    existingOrders,
                    cartOrders,
                    patientAllergies,
                    patientHomeMedications
                )
                .Select(OrderMapper.MedicationInteractionsReactions);

            return medications;
        }

        public void UpdatePatientOrderInteractionsAndReactions(long patientId, List<int>? medicationIds = null, bool? deleteExisting = false)
        {
            var siteId = _patientRepository.GetSiteIdForPatient(patientId);
            var orders = _orderRepository.GetPatientOrders(order => order.PatientId == patientId);
            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == patientId);

            IEnumerable<PatientOrder> newOrders;
            IEnumerable<PatientOrder> existingOrders;
            IEnumerable<PatientOrder> activeOrders;

            //Active orders are those not cancelled or deleted.
            //We don't need to check cancelled or deleted orders against the new orders.
            activeOrders = orders.Where
                (x => 
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            //If we have medication_ids, then use that to filter out the list of new orders versus
            //the list of orders the patient had before checking out the cart.
            if (!(medicationIds == null))
            {
                ////newOrders are the ones that contain a medication id from medicationids.
                ////and also are not cancelled or deleted.
                //newOrders = orders.Where(
                //    x => medicationIds.Contains(x.MedicationId)
                //    &&
                //        x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                //        x.OrderStatus != OrderStatus.Deleted.ToString()
                //    );

                ////existingOrders are the ones that do not contain a medication id from medicationids
                ////and also are not cancelled or deleted.
                //existingOrders = orders.Where(
                //    x => 
                //        x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                //        x.OrderStatus != OrderStatus.Deleted.ToString() &&
                //        !medicationIds.Contains(x.MedicationId)
                //    );

                //We've already got the list of orders that are not cancelled or deleted.
                //newOrders are the ones that contain a medication id from medicationids.
                newOrders = activeOrders.Where
                    (
                        x => medicationIds.Contains(x.MedicationId)
                    );

                //We've already got the list of orders that are not cancelled or deleted.
                //existingOrders are the ones that do not contain a medication id from medicationids.
                existingOrders = activeOrders.Where
                    (
                        x => !medicationIds.Contains(x.MedicationId)
                    );
            }
            else
            {
                //Just copy the current list of orders into newOrders.
                newOrders = new List<PatientOrder>(orders);
                
                //Set existngOrders to be empty.
                existingOrders = Enumerable.Empty<PatientOrder>();
            } //end if

            //Now that we've got the list of new orders, existing orders, and cart orders, pass them
            //along rather than retrieving them from the DB when we need them.
            //This should speed up the operations here.

            //Get the patient's allergies and home medications here.
            //Then pass them along to the methods farther down the call stack.
            //This prevents us from pulling these from the DB once for each order (or detail in a combo med order).
            //Winston Murdock, 09/27/2022.  PC-27110
            IEnumerable<PatientAllergy>? patientAllergies = null;
            IEnumerable<PatientHomeMedication>? patientHomeMedications = null;
            patientAllergies = _patientRepository.GetAllergiesByPatientId(patientId, a => a.IsActive && (a.ActionStatus == "C" || a.ActionStatus == "U"));
            patientHomeMedications = _homeMedicationRepository.GetPatientHomeMedications(a => a.PatientId == patientId && a.IsActive);

            foreach (var order in activeOrders)
            {
                var items = new List<MedicationModel> {
                    OrderMapper.MapOrderItemToModel(EmarOrderType.PatientOrder, order, order.PatientId, order.AddUserId, null)
                };

                //Remove this order from the list of orders so that we don't check it against itself.
                //Also remove any with the same medication ID so that we don't check one GI Cocktail order
                //against another one.
                //Winston Murdock, 09/02/2022.  PC-27472
                //ONly remove drugs that match on the medication id if this is a combo med.
                //If it's a regular med, then we don't have any worries about checking an order against itself.
                //We only have that worry when checking a combo med against another instance of the same combo med
                //when medications inside the combo med interact with each other.
                //Winston Murdock, 09/19/2022.  PC-27543
                IEnumerable<PatientOrder> newOrdersMinusThisOne;
                IEnumerable<PatientOrder> existingOrdersMinusThisOne;
                IEnumerable<PatientCartOrder> cartOrdersMinusThisOne;

                if (order.Medication.DrugId == "COMBO")
                {
                    //Combo med.
                    newOrdersMinusThisOne = newOrders.Where(o => o.Id != order.Id && o.MedicationId != order.MedicationId);
                    existingOrdersMinusThisOne = existingOrders.Where(o => o.Id != order.Id && o.MedicationId != order.MedicationId);
                    cartOrdersMinusThisOne = cartOrders.Where(co => co.MedicationId != order.MedicationId);
                }
                else
                {
                    //Not a combo med.
                    newOrdersMinusThisOne = newOrders;
                    existingOrdersMinusThisOne = existingOrders;
                    cartOrdersMinusThisOne = cartOrders;
                } //end if

                UpdateOrderInteractionsAndReactions(siteId, patientId, order.Id, order.MedicationId, order.AddUserId, EmarOrderType.PatientOrder, items, newOrdersMinusThisOne, existingOrdersMinusThisOne, cartOrdersMinusThisOne, deleteExisting, patientAllergies, patientHomeMedications);
            }
            foreach (var order in cartOrders)
            {
                var items = new List<MedicationModel> {
                    OrderMapper.MapOrderItemToModel(EmarOrderType.PatientCartOrder, order, order.PatientId, order.UserId, null)
                };

                //Remove this order from the list of orders so that we don't check it against itself.
                //If one of the meds interacts with another one, then this will show the interaction.
                //If the site has set up a combo med with meds that interact with each other,
                //then we don't want to show that interaction.
                //Winston Murdock, 09/02/2022.  PC-27472
                var newOrdersMinusThisOne = newOrders.Where(o => o.MedicationId != order.MedicationId);
                var existingOrdersMinusThisOne = existingOrders.Where(o => o.MedicationId != order.MedicationId);
                var cartOrdersMinusThisOne = cartOrders.Where(co => co.Id != order.Id && co.MedicationId != order.MedicationId);

                UpdateOrderInteractionsAndReactions(siteId, patientId, order.Id, order.MedicationId, order.UserId, EmarOrderType.PatientCartOrder, items, newOrdersMinusThisOne, existingOrdersMinusThisOne, cartOrdersMinusThisOne, deleteExisting, patientAllergies, patientHomeMedications);
            }
        }

        private void UpdateOrderInteractionsAndReactions(int siteId, long patientId, long orderId, int medicationId, int userId,
            EmarOrderType orderType, List<MedicationModel> items, IEnumerable<PatientOrder>? newOrders = null,
            IEnumerable<PatientOrder>? existingOrders = null, IEnumerable<PatientCartOrder>? cartOrders = null, bool? deleteExisting = false,
            IEnumerable<PatientAllergy>? patientAllergies = null, IEnumerable<PatientHomeMedication>? patientHomeMedications = null)
        {
            //Added newOrders, existingOrders, and CartOrders as parameters.
            //Also need to pass them down to CheckInteractionReactions.
            //Winston Murdock, 03/14/2022.  
            items[0].SiteId = siteId;
            items[0].Medication = MedicationMapper.MapMedication(_medicationRepository.GetMedication(medicationId), null);

            //If this is a combo med.
            //Get the actual medication for this medicationId.
            var medication = _medicationRepository.GetMedication(medicationId);
            if (medication.DrugId == "COMBO")
            {
                //This is a comto med.
                //Call a helper function to calculate the interactions and reactions for each
                //medication within it.
                //Winston Murdock, 08/05/2022.  PC-27326

                //Build up the fields we need in a BaseLinkResource.
                BaseLinkResource resource = new BaseLinkResource();
                resource.SiteId = siteId;
                resource.PatientId = patientId;
                resource.UserId = userId;

                //Now get the interactions/reactions for the medications inside this combo med.
                var interactionsReactions = AddInteractionsReactionsCheckForComboMedsReturnList(medication, resource, null, orderId, patientAllergies, patientHomeMedications);
                
                _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, orderId, orderType, false, deleteExisting);
            }
            else
            {
                //This is not a combo med.
                //Process normally.
                //items[0].Medication = MedicationMapper.MapMedication(_medicationRepository.GetMedication(medicationId), null);
                var interactionsReactions = CheckInteractionsReactions(userId, items, patientId, orderType == EmarOrderType.PatientCartOrder, newOrders, existingOrders, cartOrders, patientAllergies, patientHomeMedications);

                //Since we're doing targeted interaction/reaction calculations when checking out a cart (and adding to the cart).
                //And since we're handling deleting both sides of the the interactions for an order when we cancel or delete it.
                //Pass in true for the insertOnly parameter.
                //When this was false, checking out the cart was deleting all interactions between existing orders.
                //having it set to true causes this to only insert the new interactions (between the "new" orders and the
                //"existing" orders and between the "existing" orders and "new" orders).
                //Winston Murdock, 05/09/2022.  PC-27153.

                //Turns out insert only doesn't look for existing rows before inserting.
                //Thus, we were getting the same interaction listed multiple times for a given order.
                //One, inpatient patient on prod had over 7,000 interactions listed in total
                //for the "interactions" portion of the "Get Orders" query.
                //We have reverted back to passing in false for insertOnly so that we do check
                //for this interaction already existing for this order before inserting.
                //And we added a second parameter for whether or not we're deleting anything.
                //It's optional and defaults to false.
                //When it's false or missing, we won't attempt to delete anything.
                //When it's true, we will.
                //This lets us have the best of both worlds.
                //We no longer only have interactions for new orders when checking out a cart
                //(which was the initial dominoe that let us down this path) while still checking
                //for existing entries before inserting each interaction (which avoids the duplication).
                //Winston Murdock, 06/02/2022.  PC-27309
                _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, orderId, orderType, false, deleteExisting);
            } //end if
        } //end UpdateOrderInteractionsAndReactions
        #endregion

        #region Scheduler Support Methods
        public SchedulerOptionsDto GetSchedulerSetupData(int siteId, string brandName, bool bAll)
        {
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            var medications = _orderRepository
                .GetSchedulerSetupData(siteId, brandName, bAll)
                .ToList();

            if (!medications.Any() || medications.All(m => m == null))
            {
                return null;
            }

            var antimicrobialRequiredIndicators = _orderRepository.GetAntimicrobialRequiredIndicators(siteId, medications);

            var orderInstructions = new List<OrderInstruction>();

            var sharedSiteId = codeShareSites
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.OrderInstruction)?
                .SharedSiteId;

            if (sharedSiteId != null)
            {
                orderInstructions.AddRange(_orderRepository.GetOrderInstructions(sharedSiteId.Value).ToList());
            }

            //Add false for isGroupitem and empty string for pathwayToLoad.
            //Adding these required adding null for the endDateTime parameter.
            //This call previously omitted it, but we have to include it
            //since we need to pass in paramters after it in the list.
            //Winston Murdock, 10/03/2022.  PC-27538.
            var ret = OrderMapper.MapSchedulerSetupData(brandName, medications, antimicrobialRequiredIndicators, null, orderInstructions, codeShareSites, null, false, "");


            //I changed the mapper to grab the brand name from the first (and only) medication detail for each available form strength
            //and to grab the dose form and strength from the first (and only) fdb ndc info for each available form strength.
            //Then I sort the available form strength list (i.e. the meds we show to the user) by those three fields.
            //Lastly, I set the available form strength list in the return variable to the version I just sorted.
            //Winston Murdock, 10/25/2022.  PC-27618
            if (ret.AvailableFormStrength.Any())
            {
                ret.AvailableFormStrength = ret.AvailableFormStrength.OrderBy(x => x.BrandName).ThenBy(x => x.DoseForm).ThenBy(x => x.Strength).ToList();
            } //end if

            //Return.
            return ret;
        }

        public SchedulerOptionsDto GetSchedulerSetupData(int siteId, EmarOrderType itemType, int itemId, int? duration = null, int? durationUnitId = null, DateTimeOffset? endDateTime = null)
        {
            //need to pull the code share sites at the top so that we can 
            //correctly pull the data from the site we sharing groups from.
            //Winston Murdock, 08/23/2021.  EMAR-1167.
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId);

            //Get the the code share site id for groups.
            var groupSharedSiteId = codeShareSites
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.Service)?
                .SharedSiteId;

            //Use the groups code share site id to get the data here.
            //Since we're shaering from Tomball on prod, this will pass in
            //Tomball's site id instead of the id of the site we're logged in to.
            var medications = _orderRepository
                .GetSchedulerSetupData(groupSharedSiteId.Value, itemType, itemId)
                .ToList();

            if (!medications.Any() || medications.All(m => m == null))
            {
                return null;
            }

            //If this is a quick list item...
            //If the duration and duration_unit_id parameters are null, then attempt to grab those values from quickListItem.
            //If they have a value, then use them.
            //Winston Murdock, 08/24/2021.  EMAR-1162
            if (itemType == EmarOrderType.UserQuickListItem)
            {
                //This is a quick list item.
                //Get the entity from the DB.
                var quickListItem = _orderRepository.GetUserQuickListItem(itemId);

                if (!duration.HasValue)
                {
                    //Grab the duration from the object.
                    //If it's null, then fine.
                    //If it's not null, then it will be whatever is in the DB.
                    duration = quickListItem.Duration;
                } //end if

                if (!durationUnitId.HasValue)
                {
                    //Grab the duration unit id from the object.
                    //IF it's null, then fine.
                    //If it's not null, then it will be whatever is in the DB.
                    durationUnitId = quickListItem.DurationUnitId;
                } //end if
            } //end if


            //IF this is a patient order or a patient cart order. and we don't have an end datetime
            //then grab the order info from the DB and then grab its EndDateTime.
            //Winston Murdock, 02/15/2022.  PC-27021
            if (!endDateTime.HasValue)
            {
                //We do not have an end date time value.
                //See if this is a patient order or patient cart order.
                if (itemType == EmarOrderType.PatientOrder)
                {
                    //Patient Order.
                    var thisOrder = _orderRepository.GetOrder(itemId);
                    endDateTime = thisOrder.EndDateTime;
                }
                else if (itemType == EmarOrderType.PatientCartOrder)
                {
                    //Patient Cart Order.
                    var thisCartOrder = _cartOrderRepository.GetOrder(itemId);
                    endDateTime = thisCartOrder.EndDatetime;
                } //end if
            } //end if

            var antimicrobialRequiredIndicators = _orderRepository.GetAntimicrobialRequiredIndicators(siteId, medications);
            var administrations = _orderRepository.GetSchedulerAdministrations(siteId, itemType, itemId, _siteRepository.DateTimeOffsetNow(siteId), endDateTime, duration, durationUnitId);

            var orderInstructions = new List<OrderInstruction>();

            var sharedSiteId = codeShareSites
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.OrderInstruction)?
                .SharedSiteId;

            if (sharedSiteId != null)
            {
                orderInstructions.AddRange(_orderRepository.GetOrderInstructions(sharedSiteId.Value).ToList());
            }

            //Added the end date time as an optional parameter into the mapper.
            //If we have an end date time (either it was passed in from the UI or we pulled it from the DB
            //for a patient order or patient cart order), then pass it to the mapper so that it gets returned to the UI.
            //Else, it will come in as null and that field will be null in the JSON going back to the UI.
            //Winston Murdock, 02/15/2022.  PC-27021

            //If this is a group item, then we need to pass in true for isGroupitem
            //and the name of the group item's pathway to the mapper.
            //Else, we need to pass in false for isGroupitem
            //and empty string for pathwayToLoad.
            //We also need to define the return variable outside the if statement.
            //Winston Murdock, 10/03/2022.  PC-27538.
            SchedulerOptionsDto ret;

            if (itemType == EmarOrderType.GroupRememberedOrder)
            {
                //I cannot figure out a way around this extra DB hit here.
                var gli = _orderRepository.GetGroupRememberedOrderItem(itemId);
                ret = OrderMapper.MapSchedulerSetupData(medications.FirstOrDefault()?.DisplayName, medications, antimicrobialRequiredIndicators, administrations, orderInstructions, codeShareSites, endDateTime, true, gli.GroupName);
            }
            else
            {
                ret = OrderMapper.MapSchedulerSetupData(medications.FirstOrDefault()?.DisplayName, medications, antimicrobialRequiredIndicators, administrations, orderInstructions, codeShareSites, endDateTime, false, "");
            } //end if;


            //Return.
            return ret;
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
            //Use sharedSiteId rather than siteId so that we pull the routes
            //from the site we're code sharing from.
            //Winston Murdock, 04/21/2021.  EMAR-811
            return _orderRepository.GetRoutes(sharedSiteId.Value)
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

        public IEnumerable<FrequencyScheduleAdministrationDto> GetNewAdministrations(int siteId, int frequencyId, DateTimeOffset? start, DateTimeOffset? stop, int? duration = null, int? durationUnitId = null)
        {
            return _orderRepository.GetNewAdministrations(siteId, frequencyId, start ?? _siteRepository.DateTimeOffsetNow(siteId), stop, duration, durationUnitId)
                .Select(OrderMapper.MapFrequencyScheduleAdministration);
        }
        #endregion Scheduler Support Methods

        public IEnumerable<DurationUnitDto> GetDurationUnits()
        {
            return _orderRepository.GetDurationUnits()
                .Select(OrderMapper.MapDurationUnit);
        }

        public GroupListItemDto GetGroupRememberedOrderItem(long itemId)
        {
            var groupItem = _orderRepository.GetGroupRememberedOrderItem(itemId);

            var codeShareSites = _orderRepository.GetCodeShareSites(groupItem.SiteId).ToList();

            return OrderMapper.MapGroupListItem(groupItem, new BaseLinkResource(), codeShareSites);
        } //end GetGroupRememberedOrderItem

        public MedicationInteractionReaction CompressComboMedDtoInteractionsToOneEntry(GroupListItemDto groupItemDto, int userId, List<MedicationInteractionReaction> currentList)
        {
            MedicationInteractionReaction ret = new MedicationInteractionReaction
            {
                SiteId = groupItemDto.SiteId,
                UserId = userId,
                SourceTable = "group_list_items",
                SourceTableId = groupItemDto.Id,
                Type = EmarOrderType.GroupRememberedOrder,
                BrandName = groupItemDto.GroupName,
                ActiveName = groupItemDto.GroupName,
                ActiveId = groupItemDto.Id.ToString() //We don't have an FDB id here.  So possibly the group's ID will work?
            };

            //Loop through the interactions and reactions so that we have only one entry for them, not multiple.
            foreach (var item in currentList)
            {
                if (item.Interactions.Any())
                {
                    ret.Interactions.AddRange(item.Interactions);
                } //end if

                if (item.Reactions.Any())
                {
                    ret.Reactions.AddRange(item.Reactions);
                } //end if
            } //end foreach

            //Return the one MedicationInteractionReaction entity.
            return ret;
        } //end CompressComboMedDtoInteractionsToOneEntry
    }
}
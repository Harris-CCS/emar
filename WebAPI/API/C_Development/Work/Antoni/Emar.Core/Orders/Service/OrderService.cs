using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
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
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Constants = Emar.Core.Orders.Model.Constants;

namespace Emar.Core.Orders.Service
{
    public partial class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartOrderRepository _cartOrderRepository;
        private readonly IHomeMedicationRepository _homeMedicationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOptionRepository _optionRepository;
        private readonly IInteractionRepository _interactionRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICartOrderRepository cartRepository,
            IHomeMedicationRepository homeMedicationRepository,
            IPatientRepository patientRepository,
            IOptionRepository optionRepository,
             IInteractionRepository interactionRepository)
        {
            _orderRepository = orderRepository;
            _cartOrderRepository = cartRepository;
            _homeMedicationRepository = homeMedicationRepository;
            _patientRepository = patientRepository;
            _optionRepository = optionRepository;
            _interactionRepository = interactionRepository;
        }

        public PagedList<PatientOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            var orders = _orderRepository.GetOrders(patientId, resourceParameters);
            throw new NotImplementedException("Paging Disabled - to re-enable, make AssignNextActionTimeToOrders() work with a paged list of orders");
            //AssignNextActionTimeToOrders(orders);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            var dateFormat = _optionRepository.GetOption(orders[0].Patient.SiteId, OptionNames.LONG_DATE_FORMAT);
            var drugDBVendor = _optionRepository.GetOption(orders[0].Patient.SiteId, OptionNames.DRUG_DB_VENDOR);

            var ordersList = orders.Select(order => OrderMapper.MapOrder(order, dateFormat, drugDBVendor)).ToList();

            return new PagedList<PatientOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public IEnumerable<PatientOrderDto> GetOrders(long patientId)
        {
            var orders = _orderRepository.GetOrders(patientId).ToList();

            var siteId = _patientRepository.GetSiteIdForPatient(patientId);

            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);
            var drugDBVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);

            return orders.Select(order => OrderMapper.MapOrder(order, dateFormat, drugDBVendor)).ToList()
                // sort all the orders that don't have a "Next Action Time" to the bottom of the list
                .OrderBy(o => o.NextActionTime == null ? 1 : 0)
                .ThenBy(o => o.NextActionTime);
        }

        public PatientOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            var order = _orderRepository.GetOrder(orderId, resourceParameters);

            if (order == null)
            {
                return null;
            }

            var siteId = _patientRepository.GetSiteIdForPatient(order.PatientId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);
            var drugDBVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);

            var orderDto = OrderMapper.MapOrder(order, dateFormat, drugDBVendor);

            return orderDto;
        }

        public IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId)
        {
            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(orderId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);
            var administrations = _orderRepository.GetAdministrations(orderId);

            return administrations
                .Select(administration => OrderMapper.MapOrderAdministration(administration, dateFormat)).ToList();
        }

        public OrderAdministrationDto GetAdministration(long administrationId)
        {
            var administration = _orderRepository.GetAdministration(administrationId);
            var siteId = _orderRepository.GetSiteForOrder(administration.PatientOrderId);
            var administrationDto = OrderMapper.MapOrderAdministration(administration,
                _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT));

            return administrationDto;
        }

        public IEnumerable<OrderEventDto> GetEvents(long orderId)
        {
            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(orderId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);

            var events = _orderRepository.GetEvents(orderId);

            return events.Select(@event => OrderMapper.MapOrderEvent(@event, dateFormat)).ToList();
        }

        public OrderEventDto GetEvent(long eventId)
        {
            var @event = _orderRepository.GetEvent(eventId);

            // Get the Site's LongDateFormat Option
            var siteId = _orderRepository.GetSiteForOrder(@event.PatientOrderId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);

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
                dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);
            }

            return events.Select(@event => OrderMapper.MapOrderEvent(@event, dateFormat)).ToList();
        }

        #region User Quick List Services
        public UserQuickListFrameworkDto GetInitialUserQuickList(in int userId, int? siteId,
            string tabLinkBase, string orderLinkBase)
        {
            var tabList = _orderRepository.GetUserQuickListTabs(userId, siteId).OrderBy(i => i.Key).ToList();

            if (!tabList.Any())
                return null;

            // Compress all non-alpha values into "#"
            int numAlphas = 0;
            for (int i = tabList.Count - 1; i >= 0; i--)
            {
                if (!char.IsLetter(Convert.ToChar(tabList[i].Key)))
                {
                    numAlphas += tabList[i].Value;
                    tabList.RemoveAt(i);
                }
            }

            if (numAlphas > 0)
                tabList.Add(new KeyValuePair<string, int>("#", numAlphas));

            var mostUsedItems = _orderRepository.GetUserQuickListMostUsed(userId, siteId).ToList();
            List<UserQuickListItemDto> firstTabContents;
            if (mostUsedItems.Any())
            {
                firstTabContents = mostUsedItems.Select(item => OrderMapper.MapUserQuickListItem(item, orderLinkBase))
                    .OrderBy(i => i.BrandName).ToList();
                tabList.Insert(0, new KeyValuePair<string, int>(Constants.MostUsedTabTitle, mostUsedItems.Count()));
            }
            else
            {
                var items = _orderRepository.GetUserQuickListTabItems(userId, siteId, tabList[0].Key).ToList();

                firstTabContents = items.Select(dbObj => OrderMapper.MapUserQuickListItem(dbObj, orderLinkBase))
                    .OrderBy(i => i.BrandName).ToList();
            }

            ///IEnumerable<MedicationInteractionReaction> inter = interactionchaecking(firstTabContents + map to dto)
            ///IEnumerable<MedicationInteractionReaction> interactionsReactions = CheckOrderInteractions(in userId, new List<MedicationModel> { OrderMapper.MapOrderItemToModel(EmarOrderType.UserQuickListItem, quickListItem, patientId, _orderRepository) }, patientId);
            /// cannot do drug interactions checking here; we don't the patientId

            var ret = new UserQuickListFrameworkDto(firstTabContents, tabList, tabLinkBase);

            return ret;
        }

        public IEnumerable<UserQuickListItemDto> GetQuickListTab(in int userId, int? siteId, long patientId, string orderLinkBase, string tab)
        {
            List<UserQuickListItem> tabItems;
            tabItems = tab == Constants.MostUsedTabTitle
                ? _orderRepository.GetUserQuickListMostUsed(userId, siteId).ToList()
                : _orderRepository.GetUserQuickListTabItems(userId, siteId, tab).ToList();

            if (!tabItems.Any())
                return null;

            var orderedTabItems = tabItems.Select(item => OrderMapper.MapUserQuickListItem(item, orderLinkBase))
                .OrderBy(i => i.BrandName)
                .ToList();

            if (siteId != null)
            {
                orderedTabItems = AddInteractions(orderedTabItems.ToList(), siteId ?? 0, userId, patientId);
            }

            return orderedTabItems;
        }

        private List<UserQuickListItemDto> AddInteractions(List<UserQuickListItemDto> orderedTabItems, int siteId, int userId, long patientId)
        {
            var drugDBVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);

            for (var i = 0; i < orderedTabItems.Count(); i++)
            {
                var item = orderedTabItems[i];

                IEnumerable<MedicationInteractionReaction> interactionsReactions = CheckOrderInteractions(in userId, new List<MedicationModel> { OrderMapper.MapOrderItemDtoToModel(EmarOrderType.UserQuickListItem, item, patientId, _orderRepository) }, patientId);

                foreach (var interaction in interactionsReactions.SelectMany(interactionReaction => interactionReaction.Interactions))
                {
                    var medicationInteraction = new MedicationInteraction
                    {
                        InteractionDrug1 = interaction.GetValueOrDefault("drug_id_1"),
                        InteractionDrug2 = interaction.GetValueOrDefault("drug_id_2"),
                        Severity = byte.TryParse(interaction.GetValueOrDefault("severity_id"), out byte byteValue) ? byteValue : (byte)0
                    };

                    medicationInteraction.OrderInteractions.Add(
                      new OrderInteraction
                      {
                          MedicationInteractionId = medicationInteraction.Id,
                          DrugNum = 1
                      });

                    long? id = long.TryParse(interaction.GetValueOrDefault("SourceTableId2"), out long number) ? number : (long?)null;

                    switch (interaction.GetValueOrDefault("SourceTable2"))
                    {
                        case SourceTables.PatientOrders:
                            medicationInteraction.OrderInteractions.Add(
                              new OrderInteraction
                              {
                                  MedicationInteractionId = medicationInteraction.Id,
                                  DrugNum = 2,
                                  PatientOrderId = id
                              });
                            break;
                        case SourceTables.PatientCartOrders:
                            medicationInteraction.OrderInteractions.Add(
                              new OrderInteraction
                              {
                                  MedicationInteractionId = medicationInteraction.Id,
                                  DrugNum = 2,
                                  PatientCartOrderId = id
                              });
                            break;
                        case SourceTables.PatientHomeMedications:
                            medicationInteraction.OrderInteractions.Add(
                              new OrderInteraction
                              {
                                  MedicationInteractionId = medicationInteraction.Id,
                                  DrugNum = 2,
                                  PatientHomeMedicationId = id
                              });
                            break;
                    }

                    item.AddMedicationInteraction(MedicationMapper.MapMedicationInteraction(medicationInteraction, drugDBVendor));
                }
            }

            return orderedTabItems;
        }

        public CartOrderDto CopyQuickListItemToCart(in int userId, in int quickListItemId, long patientId)
        {
            var quickListItem = _orderRepository.GetUserQuickListItem(quickListItemId);
            if (quickListItem == null)
                return null;

            PatientCartOrder cartOrder = OrderMapper.MapUserQuickListItemToPatientCartOrder(quickListItem);
            cartOrder.PatientId = patientId;
            cartOrder.UserId = userId;

            IEnumerable<FrequencyScheduleAdministration> admins = _orderRepository.GetNewAdministrations(cartOrder.FrequencyScheduleId ?? -1, DateTimeOffset.Now, null);

            foreach (var admin in admins)
            {
                cartOrder.CartOrderAdministrations
                    .Add(CartOrderMapper.MapFrequencyScheduleAdminToCartOrderAdmin(admin));
            }

            PatientCartOrder newCartOrder = _cartOrderRepository.AddCartOrder(cartOrder);
            IEnumerable<MedicationInteractionReaction> interactionsReactions = CheckOrderInteractions(in userId, new List<MedicationModel> { OrderMapper.MapOrderItemToModel(EmarOrderType.UserQuickListItem, quickListItem, patientId, _orderRepository) }, patientId);
            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);
            newCartOrder = _cartOrderRepository.GetOrder(newCartOrder.Id, null);
            var siteId = _patientRepository.GetSiteIdForPatient(newCartOrder.PatientId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);
            var drugDBVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);

            var ret = CartOrderMapper.MapCartOrder(newCartOrder, dateFormat, drugDBVendor);
            return ret;

            /////////return CartOrderMapper.MapCartOrder(newCartOrder, dateFormat, drugDBVendor);
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

        public CartOrderDto CopyDepartmentPreferredItemToCart(in int userId, int departmentPreferredItemId, long patientId)
        {
            DepartmentPreferredListItem departmentPreferredListItem = _orderRepository.GetDepartmentPreferredItem(departmentPreferredItemId);

            if (departmentPreferredListItem == null)
            {
                return null;
            }

            //////IEnumerable<MedicationInteractionReaction> interactionsReactions = CheckOrderInteractions(in userId, new List<MedicationModel> { OrderMapper.MapOrderItemToModel(EmarOrderType.DepartmentPreferredListItem, departmentPreferredItemId, patientId, _orderRepository) }, patientId);
            //////_interactionRepository.RecordNewInteractions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);

            return null;
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

        public CartOrderDto CopyGroupRememberedOrderItemToCart(in int userId, int groupListItemId, long patientId)
        {
            GroupListItem groupListItem = _orderRepository.GetGroupRememberedOrderItem(groupListItemId);

            if (groupListItem == null)
            {
                return null;
            }

            //////IEnumerable<MedicationInteractionReaction> interactionsReactions = CheckOrderInteractions(in userId, new List<MedicationModel> { OrderMapper.MapOrderItemToModel(EmarOrderType.GroupRememberedOrder, groupListItemId, patientId, _orderRepository) }, patientId);
            //////_interactionRepository.RecordNewInteractions(interactionsReactions, newCartOrder.Id, EmarOrderType.PatientCartOrder);

            return null;
        }
        #endregion

        #region Drug Interactions & Allergies
        internal IEnumerable<MedicationInteractionReaction> CheckOrderInteractions(in int userId, List<MedicationModel> medicationList, long patientId)
        {
            if (medicationList.Count < 1)
            {
                return null;
            }

            IEnumerable<MedicationInteractionReaction> medications =
                MedicationManager.AddInteractionsAndReactionsToMedications(
                    userId,
                    medicationList[0].SiteId,
                    patientId,
                    //new List<MedicationModel> { item },
                    medicationList,
                    _orderRepository,
                    _cartOrderRepository,
                    _homeMedicationRepository,
                    _patientRepository,
                    _optionRepository)
                    .Select(medication => OrderMapper.MedicationInteractionsReactions(medication));

            return medications;
        }
        #endregion
    }
}
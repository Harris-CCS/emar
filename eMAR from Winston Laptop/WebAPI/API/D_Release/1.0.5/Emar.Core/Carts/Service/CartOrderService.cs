using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Carts.Repository;
using Emar.Core.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Model.Mappings;
using Emar.Core.Medications.Repository;
using Emar.Core.Medications.Service;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Patients.Repository;
using Emar.Core.ResourceParameters;
using Emar.Core.Users.Service;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Service
{
    public class CartOrderService : ICartOrderService
    {
        private readonly ICartOrderRepository _cartOrderRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOptionRepository _optionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IInteractionRepository _interactionRepository;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IMedicationService _medicationService;

        public CartOrderService(ICartOrderRepository cartOrderRepository, IPatientRepository patientRepository,
            IOptionRepository optionRepository, IOrderRepository orderRepository, IUserService userService,
            IOrderService orderService,
            IMedicationService medicationService, IMedicationRepository medicationRepository, IInteractionRepository interactionRepository)
        {
            _cartOrderRepository = cartOrderRepository;
            _patientRepository = patientRepository;
            _optionRepository = optionRepository;
            _orderRepository = orderRepository;
            _userService = userService;
            _orderService = orderService;
            _medicationService = medicationService;
            _medicationRepository = medicationRepository;
            _interactionRepository = interactionRepository;
        }

        public PagedList<CartOrderDto> GetOrders(BaseLinkResource resource)
        {
            var orders = _cartOrderRepository.GetOrders(resource);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            var siteId = _patientRepository.GetSiteIdForPatient(resource.PatientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var ordersList = orders.Select(order => CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites, resource)).ToList();
            ordersList.ForEach(r => r.OrderInteractions = r.OrderInteractions?.Distinct(new OrderInteractionDtoComparer()).ToList());

            return new PagedList<CartOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public CartOrderDto GetOrder(long orderId, BaseLinkResource resource)
        {
            var order = _cartOrderRepository.GetOrder(orderId);

            if (order == null)
                return null;

            var siteId = _patientRepository.GetSiteIdForPatient(order.PatientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var orderDto = CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites, resource);

            return orderDto;
        }

        public CartOrderDto AddCartOrder(CartOrderIuDto cartOrderAddDto, BaseLinkResource resource)
        {
            if (cartOrderAddDto.FrequencyId != null
                && cartOrderAddDto.FrequencySchedule == null)
            {
                cartOrderAddDto.FrequencySchedule = OrderMapper.MapFrequencySchedule(_cartOrderRepository.GetFrequency(cartOrderAddDto.FrequencyId.Value));
            }

            var siteId = _patientRepository.GetSiteIdForPatient(resource.PatientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
 
            var order = CartOrderMapper.MapCartOrderDto(cartOrderAddDto, drugDbVendor);

            //Before adding the order to the context/DB, call a method
            //to get the NDC from the match table.
            //if it's in there, and all three values aren't 0/0/0, we'll grab it
            //Else, we'll return null.
            //Winston Murdock, 05/11/2021.  EMAR-932.
            //Don't attempt to grab the ndc from the match table by site and medication.
            //The UI will pass in the ndc.
            //Winston Murdock, 05/13/2021.  EMAR-932.
            //order.Ndc = _cartOrderRepository.GetMatchNdcByMedIdAndSiteId(order.MedicationId, siteId);

            order = _cartOrderRepository.AddCartOrder(order);

            if (order == null)
                return null;

            resource.SiteId = siteId;

            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(resource.SiteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            var items = new List<MedicationModel>();
            items.Add(
                OrderMapper.MapOrderItemToModel(EmarOrderType.PatientCartOrder, order, order.PatientId, order.UserId, codeShareSiteMedicationUnit)
            );
            items[0].SiteId = resource.SiteId;
            items[0].Medication = MedicationMapper.MapMedication(_medicationRepository.GetMedication(order.MedicationId), codeShareSiteMedicationUnit);

            var interactionsReactions = _orderService.CheckInteractionsReactions(
                resource.UserId,
                items,
                resource.PatientId);
            _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, order.Id, EmarOrderType.PatientCartOrder);

            var orderDto = CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites, resource);

            return orderDto;
        }

        public bool UpdateCartOrder(CartOrderIuDto cartOrderUpdateDto)
        {
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrderUpdateDto.PatientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var order = CartOrderMapper.MapCartOrderDto(cartOrderUpdateDto, drugDbVendor);

            return _cartOrderRepository.UpdateCartOrder(order);
        }

        public bool DeleteCartOrder(long cartOrderId)
        {
            return _cartOrderRepository.DeleteCartOrder(cartOrderId);
        }

        public bool DeleteCartOrders(int userId, long patientId)
        {
            return _cartOrderRepository.DeleteCartOrders(userId, patientId);
        }

        public CartPreCheckoutRequestDataDto GetCartPreCheckoutData(int userId, long patientId)
        {
            var orders = _cartOrderRepository
                .GetPatientCartOrders(order =>
                    order != null &&
                    order.UserId == userId &&
                    order.PatientId == patientId &&
                    (order.OrderInteractions.Count > 0 ||
                     order.OrderReactions.Count > 0),
                    true)
                .ToList();

            var siteId = _patientRepository.GetSiteIdForPatient(patientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();
            var overrideReasons = new List<OverrideReason>();

            //Need to get the override reason site id and use that instead of the logged in site id.
            //Winston Murdock, 05/06/2021.  EMAR-811.
            var sharedSiteId = _orderRepository.GetCodeShareSites(siteId)
            .FirstOrDefault(c =>
                c.Entity == OrderRepository.CodeShareEntity.OverrideReason)?
            .SharedSiteId;

            //If we don't find an entry in the site_code_shares table, use the current site id.
            if (sharedSiteId == null)
            {
                sharedSiteId = siteId;
            }

            overrideReasons = _cartOrderRepository
                .GetOverrideReasons(sharedSiteId.Value)
                .ToList();

            var checkoutData = new CartPreCheckoutRequestDataDto
            {
                //Use the new method that takes in the userId in addition to siteId and patientId.
                //Winston Murdock, 01/18/2022.  PC-26918
                //OrderingPhysicianData = _userService
                //    .GetOrderingPhysicians(siteId, patientId),
                OrderingPhysicianData = _userService
                    .GetOrderingPhysicians(siteId, patientId, userId),

                DrugInteractionOrders = orders
                    .Where(order => order.OrderInteractions.Count > 0)
                    .Select(order => CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites))
                    .ToList(),

                DrugInteractionOverrideReasons = overrideReasons
                    .Where(reason => reason.IsMedication)
                    .Select(CartOrderMapper.MapOverrideReason)
                    .ToList(),

                AllergyReactionOrders = orders
                    .Where(order => order.OrderReactions.Count > 0)
                    .Select(order => CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites))
                    .ToList(),

                AllergyReactionOverrideReasons = overrideReasons
                    .Where(reason => reason.IsMedication == false)
                    .Select(CartOrderMapper.MapOverrideReason)
                    .ToList()
            };

            return checkoutData;
        }

        public bool CheckoutOrders(CartPreCheckoutResponseDataDto cartPreCheckoutResponseData, int userId, long patientId)
        {
            return _cartOrderRepository.CheckoutOrders(cartPreCheckoutResponseData, userId, patientId);
        }

        public IEnumerable<CartOrderAdministrationDto> GetAdministrations(long orderId)
        {
            throw new NotImplementedException("Method doesn't appear to be needed.");
            //var administrations = _orderRepository.GetAdministrations(orderId);
            //var administrationsList = administrations.Select(administration => CartOrderMapper.MapCartOrderAdministration(administration)).ToList();
            //////////var administrationsList = new List<CartOrderAdministrationDto>();

            //////////foreach (var administration in administrations)
            //////////{
            //////////    administrationsList.Add(CartOrderMapper.MapCartOrderAdministration(administration));
            //////////}

            //return administrationsList;
        }

        public CartOrderAdministrationDto GetAdministration(long administrationId)
        {
            throw new NotImplementedException("Method doesn't appear to be needed.");
            //var administration = _orderRepository.GetAdministration(administrationId);
            //var administrationDto = CartOrderMapper.MapCartOrderAdministration(administration);

            //return administrationDto;
        }
    }
}
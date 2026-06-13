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
using Emar.Core.Orders.Model;
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

        public List<int> GetAllMedicationIdsInCart(BaseLinkResource resource)
        {
            var orders = _cartOrderRepository.GetOrders(resource);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            //Now that we have all of the cart orders in this cart, loop through the list and get the medicationId.
            List<int> ret = new List<int>();

            foreach (PatientCartOrder pco in orders)
            {
                ret.Add(pco.MedicationId);
            } //end foreach.

            return ret;
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
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            //Moving the logic to recalculate the interactions and reactions for all cart orders
            //(including the one that was just added) into a helper function.
            //Adding UserId to this call so that we only recalculate for this user/patient and not
            //for any other user's carts for this patient.
            //Winston Murdock, 04/18/2022.  PC-27058
            RecalculateCartOrderInteractionsAndReactions(resource.PatientId, siteId, resource.UserId);

            //var items = new List<MedicationModel>();
            //items.Add(
            //    OrderMapper.MapOrderItemToModel(EmarOrderType.PatientCartOrder, order, order.PatientId, order.UserId, codeShareSiteMedicationUnit)
            //);
            //items[0].SiteId = resource.SiteId;
            //items[0].Medication = MedicationMapper.MapMedication(_medicationRepository.GetMedication(order.MedicationId), codeShareSiteMedicationUnit);

            //var interactionsReactions = _orderService.CheckInteractionsReactions(
            //    resource.UserId,
            //    items,
            //    resource.PatientId);
            //_interactionRepository.RecordNewInteractionsReactions(interactionsReactions, order.Id, EmarOrderType.PatientCartOrder


            var orderDto = CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites, resource);

            return orderDto;
        }

        public bool UpdateCartOrder(CartOrderIuDto cartOrderUpdateDto)
        {
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrderUpdateDto.PatientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var order = CartOrderMapper.MapCartOrderDto(cartOrderUpdateDto, drugDbVendor);

            //We do not need to recalculate the interactions and reactions here.
            //The user editted the frequency, dose, unit, route, etc...
            //But they didn't change to an entirely new drug.
            //So the interactions and reactions will not have changed.

            return _cartOrderRepository.UpdateCartOrder(order);
        }

        public bool DeleteCartOrder(long cartOrderId)
        {
            //Insted of merely returning, save the return value in a variable.
            //Then recalculate interactions/reactions for all cart orders for this patient/user.
            //Then return the return value.

            //return _cartOrderRepository.DeleteCartOrder(cartOrderId);
            
            //1) Get the cartOrder
            var cartOrder = _cartOrderRepository.GetOrder(cartOrderId);

            //2) Get the patient's siteId.
            var siteId = _patientRepository.GetSiteIdForPatient(cartOrder.PatientId);

            //3) Actually delete the cart order.  Store the return value so that we can return it later.
            var bRet = _cartOrderRepository.DeleteCartOrder(cartOrderId);

            //4) Call a helper function to recalculate the interactions and reactions for all cart orders.
            //We don't need to recalculate here.  We only deleted interactions for the cart order that was
            //just deleted (both sides of it).  Thus, the interaction for this patient's cart are already correct.
            //RecalculateCartOrderInteractionsAndReactions(cartOrder.PatientId, siteId, cartOrder.UserId);

            //5) Return.
            return bRet;
        }

        public bool DeleteCartOrders(int userId, long patientId)
        {
            //Insted of merely returning, save the return value in a variable.
            //Then recalculate interactions/reactions for all cart orders for this patient/user.
            //Then return the return value.

            //return _cartOrderRepository.DeleteCartOrders(userId, patientId);

            //1) Get the patient's siteId.
            var siteId = _patientRepository.GetSiteIdForPatient(patientId);

            //2) Actually delete the cart orders.  Store the return value so that we can return it later.
            var bRet = _cartOrderRepository.DeleteCartOrders(userId, patientId);

            //3) Call a helper function to recalculate the interactions and reactions for all cart orders.
            //We don't need to recalculate here.  We just deleted each cart order for this user/patient,
            //and deleting a cart order handles deleting its interactions.
            //RecalculateCartOrderInteractionsAndReactions(patientId, siteId, userId);

            //4) Return.
            return bRet;
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

        public void RecalculateCartOrderInteractionsAndReactions(long patientId, int siteId, int userId)
        {
            //This function recalculates the interactions and reactions for all cart orders for a given patient.
            //It is called by three different endpoint.
            //1) Add a cart order.
            //2) Delete a cart order.
            //3) Delete all orders from a patient/user's cart.
            //We don't need to call it when updating a cart order as they aren't changing the medication
            //but are only changing the dose, unit, route, frequency, etc...
            //Winston Murdock, 04/13/2022.  PC-27058

            //Per Romel, we don't want to calculate interactions between cart orders in one user's
            //cart to and cart orders in another user's cart for the same patient.
            //Thus, I added the userId as a paramter.
            //And I'll only pull the list of cart orders for this user (and not all users).
            //Winston Murdock, 04/18/2022.  PC-27058

            //1) Get the codeshare site for MedicationUnit since we need it below.
            var codeShareSiteMedicationUnit = _orderRepository.GetCodeShareSites(siteId)
                .FirstOrDefault(c =>
                    c.Entity == OrderRepository.CodeShareEntity.MedicationUnit)?
                .SharedSiteId;

            //2) Get all of the patientOrders for this patient (we aren't recalculating things
            //for them but this lets us only get them once instead of multiple times since
            //we do need to show any interactions/reactinos that any of the cart orders have to them).
            var patientOrders = _orderRepository.GetPatientOrders(order => order.PatientId == patientId);

            //Need to filter out the canceled and deleted orders here.
            //Winston Murdock, 05/03/2022.  PC-27193
            patientOrders = patientOrders.Where
                (x =>
                    x.OrderStatus != OrderStatus.Cancelled.ToString() &&
                    x.OrderStatus != OrderStatus.Deleted.ToString()
                );

            //3) Get all of the cartOrders for this patient/user (we are recalculating for each of them).
            var cartOrders = _cartOrderRepository.GetPatientCartOrders(order => order.PatientId == patientId && order.UserId == userId);
            
            //4) Loop through the cartOrders f0r this patient/user.
            foreach (var cartOrder in cartOrders)
            {
                //5) Call the recalculate stuff for each cart order in this user's cart for this patient.
                //This section was copied straight from the AddCartOrder function above.
                //The items variable is intended to be a list with only one element.
                var items = new List<MedicationModel>();
                items.Add(
                    OrderMapper.MapOrderItemToModel(EmarOrderType.PatientCartOrder, cartOrder, patientId, cartOrder.UserId, codeShareSiteMedicationUnit)
                );
                items[0].SiteId = siteId;
                items[0].Medication = MedicationMapper.MapMedication(_medicationRepository.GetMedication(cartOrder.MedicationId), codeShareSiteMedicationUnit);

                //This recalculates the interactions and reactions for each item in the items list.
                //Pass in the list of patientOrders so that it doesn't try to get them from the DB
                //for each iteration through this loop.
                //Pass in the full list of cartOrders for the same reason.
                var interactionsReactions = _orderService.CheckInteractionsReactions(
                    cartOrder.UserId,
                    items,
                    patientId,
                    true,
                    null,
                    patientOrders,
                    cartOrders);
                _interactionRepository.RecordNewInteractionsReactions(interactionsReactions, cartOrder.Id, EmarOrderType.PatientCartOrder);
            } //end foreach cartOrder in the cart for this patient/user.
        } //end RecalculateCartOrderInteractionsAndReactions
    }
}
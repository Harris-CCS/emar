using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Carts.Repository;
using Emar.Core.Helpers;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;
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
        private readonly IUserService _userService;

        public CartOrderService(ICartOrderRepository cartOrderRepository, IPatientRepository patientRepository,
            IOptionRepository optionRepository, IOrderRepository orderRepository, IUserService userService)
        {
            _cartOrderRepository = cartOrderRepository;
            _patientRepository = patientRepository;
            _optionRepository = optionRepository;
            _orderRepository = orderRepository;
            _userService = userService;
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

            var order = CartOrderMapper.MapCartOrderDto(cartOrderAddDto);
            order = _cartOrderRepository.AddCartOrder(order);

            if (order == null)
                return null;

            var siteId = _patientRepository.GetSiteIdForPatient(order.PatientId);
            var drugDbVendor = _optionRepository.GetOption(siteId, OptionNames.DRUG_DB_VENDOR);
            var codeShareSites = _orderRepository.GetCodeShareSites(siteId).ToList();

            var orderDto = CartOrderMapper.MapCartOrder(order, drugDbVendor, codeShareSites, resource);

            return orderDto;
        }

        public bool UpdateCartOrder(CartOrderIuDto cartOrderUpdateDto)
        {
            var order = CartOrderMapper.MapCartOrderDto(cartOrderUpdateDto);

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

            if (orders.Any())
            {
                overrideReasons = _cartOrderRepository
                    .GetOverrideReasons(siteId)
                    .ToList();
            }

            var checkoutData = new CartPreCheckoutRequestDataDto
            {
                OrderingPhysicianData = _userService
                    .GetOrderingPhysicians(siteId, patientId),

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
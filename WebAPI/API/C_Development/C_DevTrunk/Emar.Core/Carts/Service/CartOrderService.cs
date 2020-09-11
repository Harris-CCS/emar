using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Carts.Repository;
using Emar.Core.Helpers;
using Emar.Core.Options.Model;
using Emar.Core.Options.Repository;
using Emar.Core.Patients.Repository;
using Emar.Core.ResourceParameters;

namespace Emar.Core.Carts.Service
{
    public class CartOrderService : ICartOrderService
    {
        private readonly ICartOrderRepository _orderRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOptionRepository _optionRepository;

        public CartOrderService(ICartOrderRepository orderRepository, IPatientRepository patientRepository,
            IOptionRepository optionRepository)
        {
            _orderRepository = orderRepository;
            _patientRepository = patientRepository;
            _optionRepository = optionRepository;
        }

        public PagedList<CartOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            var orders = _orderRepository.GetOrders(patientId, resourceParameters);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            var siteId = _patientRepository.GetSiteIdForPatient(patientId ?? resourceParameters.PatientId ?? -1);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);

            var ordersList = orders.Select(order => CartOrderMapper.MapCartOrder(order, dateFormat)).ToList();

            return new PagedList<CartOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public CartOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            var order = _orderRepository.GetOrder(orderId, resourceParameters);

            if (order == null)
                return null;

            var siteId = _patientRepository.GetSiteIdForPatient(order.PatientId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);
            
            var orderDto = CartOrderMapper.MapCartOrder(order, dateFormat);

            return orderDto;
        }

        public CartOrderDto AddCartOrder(CartOrderIuDto cartOrderAddDto)
        {
            var order = CartOrderMapper.MapCartOrderDto(cartOrderAddDto);
            order = _orderRepository.AddCartOrder(order);

            if (order == null)
                return null;

            var siteId = _patientRepository.GetSiteIdForPatient(order.PatientId);
            var dateFormat = _optionRepository.GetOption(siteId, OptionNames.LONG_DATE_FORMAT);

            var orderDto = CartOrderMapper.MapCartOrder(order, dateFormat);

            return orderDto;
        }

        public bool UpdateCartOrder(long? cartOrderId, CartOrderDto cartOrderDto, CartOrderIuDto cartOrderUpdateDto)
        {
            var order = CartOrderMapper.MapCartOrderDto(cartOrderUpdateDto);

            return _orderRepository.UpdateCartOrder(order);
        }

        public bool DeleteCartOrder(long? cartOrderId)
        {
            return _orderRepository.DeleteCartOrder(cartOrderId);
        }

        public bool DeleteCartOrders(int? userId, long? patientId)
        {
            return _orderRepository.DeleteCartOrders(userId, patientId);
        }

        public bool CheckoutOrders(int? userId, long? patientId)
        {
            return _orderRepository.CheckoutOrders(userId, patientId);
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

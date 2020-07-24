using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Carts.Repository;

namespace Emar.Core.Carts.Service
{
    public class CartOrderService : ICartOrderService
    {
        private readonly ICartOrderRepository _orderRepository;

        public CartOrderService(ICartOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public PagedList<CartOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            var orders = _orderRepository.GetOrders(patientId, resourceParameters);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            var ordersList = orders.Select(order => CartOrderMapper.MapCartOrder(order)).ToList();

            return new PagedList<CartOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public CartOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            var order = _orderRepository.GetOrder(orderId, resourceParameters);

            if (order == null)
            {
                return null;
            }

            var orderDto = CartOrderMapper.MapCartOrder(order);

            return orderDto;
        }

        public IEnumerable<CartOrderAdministrationDto> GetAdministrations(long orderId)
        {
            var administrations = _orderRepository.GetAdministrations(orderId);
            var administrationsList = administrations.Select(administration => CartOrderMapper.MapCartOrderAdministration(administration)).ToList();
            //////////var administrationsList = new List<CartOrderAdministrationDto>();

            //////////foreach (var administration in administrations)
            //////////{
            //////////    administrationsList.Add(CartOrderMapper.MapCartOrderAdministration(administration));
            //////////}

            return administrationsList;
        }

        public CartOrderAdministrationDto GetAdministration(long administrationId)
        {
            var administration = _orderRepository.GetAdministration(administrationId);
            var administrationDto = CartOrderMapper.MapCartOrderAdministration(administration);

            return administrationDto;
        }

        public CartOrderDto AddCartOrder(CartOrderDto cartOrderAddDto)
        {
            var order = CartOrderMapper.MapCartOrderDto(cartOrderAddDto);
            order = _orderRepository.AddCartOrder(order);

            if (order == null)
            {
                return null;
            }

            var orderDto = CartOrderMapper.MapCartOrder(order);

            return orderDto;
        }

        public bool UpdateCartOrder(long? cartOrderId, CartOrderDto cartOrderDto, CartOrderDto cartOrderUpdateDto)
        {
            var order = CartOrderMapper.MapCartOrderDto(cartOrderUpdateDto);

            return _orderRepository.UpdateCartOrder(order);
            //return _orderRepository.UpdateCartOrder(cartOrderId, cartOrderDto, cartOrderUpdateDto);
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
    }
}

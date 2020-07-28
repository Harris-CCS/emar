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

        public bool CheckoutOrders(int? userId, long? patientId)
        {
            return _orderRepository.CheckoutOrders(userId, patientId);
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
    }
}

using System.Collections.Generic;
using Emar.Core.Carts.Model;

namespace Emar.Core.Carts.Service
{
    public interface ICartOrderService
    {
        PagedList<CartOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        CartOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters);
        IEnumerable<CartOrderAdministrationDto> GetAdministrations(long orderId);
        CartOrderAdministrationDto GetAdministration(long administrationId);
        CartOrderDto AddCartOrder(CartOrderDto cartOrderAddDto);
        bool UpdateCartOrder(long? cartOrderId, CartOrderDto cartOrderDto, CartOrderDto cartOrderUpdateDto);
        bool DeleteCartOrder(long? cartOrderId);
        bool DeleteCartOrders(int? userId, long? patientId);
        bool CheckoutOrders(int? userId, long? patientId);
    }
}

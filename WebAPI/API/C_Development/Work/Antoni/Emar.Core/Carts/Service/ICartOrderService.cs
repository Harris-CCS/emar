using System.Collections.Generic;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;

namespace Emar.Core.Carts.Service
{
    public interface ICartOrderService
    {
        PagedList<CartOrderDto> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        CartOrderDto GetOrder(long orderId, OrdersResourceParameters resourceParameters);
        CartOrderDto AddCartOrder(CartOrderIuDto cartOrderAddDto);
        bool UpdateCartOrder(long? cartOrderId, CartOrderDto cartOrderDto, CartOrderIuDto cartOrderUpdateDto);
        bool DeleteCartOrder(long? cartOrderId);
        bool DeleteCartOrders(int? userId, long? patientId);
        bool CheckoutOrders(int? userId, long? patientId);


        IEnumerable<CartOrderAdministrationDto> GetAdministrations(long orderId);
        CartOrderAdministrationDto GetAdministration(long administrationId);
    }
}

using System.Collections.Generic;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;

namespace Emar.Core.Carts.Service
{
    public interface ICartOrderService
    {
        PagedList<CartOrderDto> GetOrders(BaseLinkResource resource);
        List<int> GetAllMedicationIdsInCart(BaseLinkResource resource);
        CartOrderDto GetOrder(long orderId, BaseLinkResource resource);
        CartOrderDto AddCartOrder(CartOrderIuDto cartOrderAddDto, BaseLinkResource resource);
        bool UpdateCartOrder(CartOrderIuDto cartOrderUpdateDto);
        bool DeleteCartOrder(long cartOrderId);
        bool DeleteCartOrders(int userId, long patientId);
        CartPreCheckoutRequestDataDto GetCartPreCheckoutData(int userId, long patientId);
        bool CheckoutOrders(CartPreCheckoutResponseDataDto cartPreCheckoutResponseData, int userId, long patientId);

        IEnumerable<CartOrderAdministrationDto> GetAdministrations(long orderId);
        CartOrderAdministrationDto GetAdministration(long administrationId);

        void RecalculateCartOrderInteractionsAndReactions(long patientId, int siteId, int userId);
    }
}
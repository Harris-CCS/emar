using System.Collections.Generic;
using Emar.Core.Carts.Model;

namespace Emar.Core.Carts.Service
{
    public interface ICartOrderService
    {
        bool CheckoutOrders(int? userId, long? patientId);


        IEnumerable<CartOrderAdministrationDto> GetAdministrations(long orderId);
        CartOrderAdministrationDto GetAdministration(long administrationId);
    }
}

using System.Collections.Generic;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Repository
{
    public interface ICartOrderRepository
    {
        bool CheckoutOrders(int? userId, long? patientId);


        IEnumerable<CartOrderAdministration> GetAdministrations(long orderId);
        CartOrderAdministration GetAdministration(long administrationId);
    }
}

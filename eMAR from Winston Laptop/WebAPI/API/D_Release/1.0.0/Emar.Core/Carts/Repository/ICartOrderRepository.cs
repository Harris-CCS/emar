using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Repository
{
    public interface ICartOrderRepository
    {
        PagedList<PatientCartOrder> GetOrders(BaseLinkResource resource);
        IEnumerable<PatientCartOrder> GetPatientCartOrders(Expression<Func<PatientCartOrder, bool>> wherePredicate = null, bool forOverrideReasons = false);
        PatientCartOrder GetOrder(long orderId);
        PatientCartOrder AddCartOrder(PatientCartOrder cartOrder);
        bool UpdateCartOrder(PatientCartOrder cartOrder);
        bool DeleteCartOrder(long cartOrderId);
        bool DeleteCartOrders(int userId, long patientId);
        IEnumerable<OverrideReason> GetOverrideReasons(int siteId);
        bool CheckoutOrders(CartPreCheckoutResponseDataDto cartPreCheckoutResponseData, int userId, long patientId);

        IEnumerable<CartOrderAdministration> GetAdministrations(long orderId);
        CartOrderAdministration GetAdministration(long administrationId);
        FrequencySchedule GetFrequency(int frequencyId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Emar.Core.Helpers;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;

namespace Emar.Core.Carts.Repository
{
    public interface ICartOrderRepository
    {
        PagedList<PatientCartOrder> GetOrders(long? patientId, OrdersResourceParameters resourceParameters);
        IEnumerable<PatientCartOrder> GetPatientCartOrders(Expression<Func<PatientCartOrder, bool>> wherePredicate = null);
        PatientCartOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters);
        PatientCartOrder AddCartOrder(PatientCartOrder cartOrder);
        bool UpdateCartOrder(PatientCartOrder cartOrder);
        bool DeleteCartOrder(long? cartOrderId);
        bool DeleteCartOrders(int? userId, long? patientId);
        bool CheckoutOrders(int? userId, long? patientId);

        IEnumerable<CartOrderAdministration> GetAdministrations(long orderId);
        CartOrderAdministration GetAdministration(long administrationId);
    }
}

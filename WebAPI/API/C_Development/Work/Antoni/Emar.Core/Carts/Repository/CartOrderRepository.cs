using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Emar.Core.Carts.Repository
{
    public class CartOrderRepository : ICartOrderRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public CartOrderRepository()
        {

        }

        public CartOrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public PagedList<PatientCartOrder> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            Expression<Func<PatientCartOrder, bool>> whereLambda;
            if (patientId == null)
                whereLambda = order =>
                    order.UserId == resourceParameters.UserId && order.PatientId == resourceParameters.PatientId;
            else
                whereLambda = order =>
                    order.UserId == resourceParameters.UserId && order.PatientId == patientId;

            var orders = GetCartOrders(whereLambda);

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<CartOrderDto, PatientCartOrder>();

                orders = orders.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientCartOrder>.Create(orders.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        IEnumerable<PatientCartOrder> GetCartOrders(Expression<Func<PatientCartOrder, bool>> wherePredicate)
        {
            var orders = _context.PatientCartOrders
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.MedicationRoute)
                .Include(order => order.FrequencySchedule)
                .Include(order => order.MedicationUnit)
                .Include(order => order.User)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.DrugInteractionView)
                .Include(order => order.AllergyReactionsView)
                .Where(wherePredicate)
                .AsEnumerable();

            return orders;
        }

        public IEnumerable<PatientCartOrder> GetPatientCartOrders(Expression<Func<PatientCartOrder, bool>> wherePredicate = null)
        {
            return _context.PatientCartOrders
                .Where(wherePredicate)
                .ToList()
                .Select(order =>
                {
                    order.FdbBrandName =
                    (from s in (from s in _context.FdbBrandName select s).Where(u => u.Medid.ToString() == order.DrugId)
                     select s)
                     .FirstOrDefault();
                    return order;
                })
                .AsEnumerable();
        }

        public PatientCartOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            return GetCartOrders(order => order.Id == orderId)
                    .FirstOrDefault();
        }

        public PatientCartOrder AddCartOrder(PatientCartOrder cartOrder)
        {
            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.PatientCartOrders.Add(cartOrder);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }

                return GetOrder(cartOrder.Id, null);
            }
        }

        public bool UpdateCartOrder(PatientCartOrder cartOrder)
        {
            int i = 0;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var order = _context.PatientCartOrders.First(g => g.Id == cartOrder.Id);
                    _context.Entry(order).CurrentValues.SetValues(cartOrder);

                    foreach (var administration in order.CartOrderAdministrations)
                    {
                        _context.CartOrderAdministrations.Remove(administration);
                    }

                    order.CartOrderAdministrations = cartOrder.CartOrderAdministrations;

                    i = _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    i = 0;
                    transaction.Rollback();
                }
            }

            return i > 0;
        }

        public bool DeleteCartOrder(long? cartOrderId)
        {
            var cartOrder = _context.PatientCartOrders
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.MedicationInteraction)
                .Include(order => order.OrderReactions)
                .FirstOrDefault(order => order.Id == cartOrderId);

            return DeleteCartOrder(cartOrder);
        }

        public bool DeleteCartOrders(int? userId, long? patientId)
        {
            var cartOrders = _context.PatientCartOrders
                .Where(order => order.UserId == userId)
                .Where(order => order.PatientId == patientId)
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.MedicationInteraction)
                .Include(order => order.OrderReactions)
                .ToList();

            bool success = true;

            foreach (var cartOrder in cartOrders)
            {
                if (!DeleteCartOrder(cartOrder))
                {
                    success = false;
                }
            }

            return success;
        }

        private bool DeleteCartOrder(PatientCartOrder cartOrder)
        {
            bool success = true;
            int i = 0;

            if (cartOrder != null)
            {
                if (cartOrder.OrderInteractions != null)
                {
                    var medicationInteractions = new Collection<MedicationInteraction>();

                    foreach (var orderInteraction in cartOrder.OrderInteractions)
                    {
                        if (orderInteraction.MedicationInteraction != null)
                        {
                            medicationInteractions.Add(orderInteraction.MedicationInteraction);
                        }
                    }

                    if (cartOrder.OrderInteractions != null)
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                foreach (var interaction in cartOrder.OrderInteractions)
                                {
                                    var orderInteractions = _context.OrderInteractions
                                        .Where(x => x.MedicationInteractionId == interaction.MedicationInteractionId);

                                    _context.OrderInteractions.RemoveRange(orderInteractions);
                                }

                                i = _context.SaveChanges(true);

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                success = false;
                                transaction.Rollback();
                            }
                        }
                    }

                    if (success && medicationInteractions.Count > 0)
                    {
                        using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                        {
                            try
                            {
                                foreach (var interaction in medicationInteractions)
                                {
                                    if (interaction != null)
                                    {
                                        _context.MedicationInteractions.Remove(interaction);
                                    }
                                }

                                i = _context.SaveChanges(true);

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                success = false;
                                transaction.Rollback();
                            }
                        }
                    }
                }

                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (cartOrder.CartOrderAdministrations != null)
                        {
                            _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                        }

                        if (cartOrder.OrderReactions != null)
                        {
                            _context.OrderReactions.RemoveRange(cartOrder.OrderReactions);
                        }

                        _context.PatientCartOrders.Remove(cartOrder);
                        i = _context.SaveChanges(true);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        i = 0;
                        transaction.Rollback();
                    }
                }
            }

            return success && i > 0;
        }

        public bool CheckoutOrders(int? userId, long? patientId)
        {
            int i = 0;

            var cartOrders = _context.PatientCartOrders
                .Where(order => order.UserId == userId)
                .Where(order => order.PatientId == patientId)
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.OrderInteractions)
                    .ThenInclude(interaction => interaction.MedicationInteraction)
                .Include(order => order.OrderReactions)
                .ToList();

            foreach (var cartOrder in cartOrders)
            {
                using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        _context.PatientOrders.Add(OrderMapper.MapCartOrderToOrder(cartOrder));

                        _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                        _context.PatientCartOrders.Remove(cartOrder);

                        i = _context.SaveChanges(true);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        i = 0;
                        transaction.Rollback();
                    }
                }
            }

            return i > 0;
        }

        public IEnumerable<CartOrderAdministration> GetAdministrations(long orderId)
        {
            return _context.CartOrderAdministrations
                    .Where(administration => administration.PatientCartOrderId == orderId)
                    .AsEnumerable();
        }

        public CartOrderAdministration GetAdministration(long administrationId)
        {
            return _context.CartOrderAdministrations
                    .FirstOrDefault(administration => administration.Id == administrationId);
        }

        ////////////  probably not needed, leave for now, will clean later
        ////////////public FdbBrandName GetPatientCartOrderFdbBrandName(long orderId)
        ////////////{
        ////////////    var query =
        ////////////        from p in (from p in _context.PatientCartOrders select p).Where(u => u.Id == orderId)
        ////////////        join n in _context.FdbNdcInfo on p.DrugId equals n.GcnSeqno.ToString()
        ////////////        join s in _context.FdbBrandName on n.RoutedGenId equals s.RoutedGenId
        ////////////        select s;

        ////////////    return query.FirstOrDefault();
        ////////////}
    }
}
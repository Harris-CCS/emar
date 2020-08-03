using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.ResourceParameters;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
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
            long userId = resourceParameters.UserId ?? -1;
            long ptId = (patientId ?? resourceParameters.PatientId) ?? -1;

            if ((userId == -1) ||
                (ptId == -1))
            {
                return null;
            }

            var orders = GetCartOrders(userId, ptId);

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<CartOrderDto, PatientCartOrder>();

                orders = orders.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientCartOrder>.Create(orders.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        IEnumerable<PatientCartOrder> GetCartOrders(long userId = -1, long patientId = -1)
        {
            var orders = _context.PatientCartOrders
                .Include(order => order.CartOrderAdministrations)
                .Include(order => order.MedicationRoute)
                .Include(order => order.User)
                .Include(order => order.Patient)
                    .ThenInclude(patient => patient.Site)
                        .ThenInclude(site => site.SiteOptions)
                            .ThenInclude(siteOptions => siteOptions.Option)
                .AsEnumerable();

            if (userId != -1)
            {
                orders = orders
                    .Where(order => order.UserId == userId);
            }

            if (patientId != -1)
            {
                orders = orders
                    .Where(order => order.PatientId == patientId);
            }

            return orders;
        }

        public PatientCartOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            return GetCartOrders()
                    .FirstOrDefault(order => order.Id == orderId);
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
            int i = 0;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var cartOrder = _context.PatientCartOrders
                                        .Include(order => order.CartOrderAdministrations)
                                        .FirstOrDefault(order => order.Id == cartOrderId);

                    if (cartOrder.CartOrderAdministrations != null)
                    {
                        _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
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

            return i > 0;
        }

        public bool DeleteCartOrders(int? userId, long? patientId)
        {
            int i = 0;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var cartOrders = _context.PatientCartOrders
                                        .Where(order => order.UserId == userId)
                                        .Where(order => order.PatientId == patientId)
                                        .Include(order => order.CartOrderAdministrations)
                                        .AsEnumerable();

                    foreach (var cartOrder in cartOrders)
                    {
                        if (cartOrder.CartOrderAdministrations != null)
                        {
                            _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                        }
                    }

                    _context.PatientCartOrders.RemoveRange(cartOrders);
                    i = _context.SaveChanges(true);
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

        public bool CheckoutOrders(int? userId, long? patientId)
        {
            int i = 0;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var cartOrders = _context.PatientCartOrders
                                        .Where(order => order.UserId == userId)
                                        .Where(order => order.PatientId == patientId)
                                        .Include(order => order.CartOrderAdministrations)
                                        .AsEnumerable();

                    foreach (var cartOrder in cartOrders)
                    {
                        var order = new PatientOrder
                        {
                            PatientId = cartOrder.PatientId,
                            AddUserId = cartOrder.UserId,
                            AddDatetime = cartOrder.AddDatetime,
                            Ndc = cartOrder.Ndc,
                            DrugId = cartOrder.DrugId,
                            BrandName = cartOrder.BrandName,
                            Dose = cartOrder.Dose,
                            MedicationUnitId = cartOrder.MedicationUnitId,
                            MedicationRouteId = cartOrder.MedicationRouteId,
                            Priority = cartOrder.Priority,
                            FrequencyId = cartOrder.FrequencyId,
                            Prn = cartOrder.Prn,
                            PointInTime = cartOrder.PointInTime,
                            OrderStatus = OrderStatuses.Pending.ToString(),
                            BeginDatetime = cartOrder.BeginDatetime,
                            EndDateTime = cartOrder.EndDatetime,
                            OrderNotes = cartOrder.OrderNotes
                        };

                        if (cartOrder.CartOrderAdministrations != null)
                        {
                            foreach (var cartAdministration in cartOrder.CartOrderAdministrations)
                            {
                                order.OrderAdministrations.Add(
                                new OrderAdministration
                                {
                                    PointInTime = cartAdministration.PointInTime,
                                    AdministrationScheduledDatetime = cartAdministration.AdministrationScheduledDatetime,
                                    StopScheduledDatetime = cartAdministration.StopScheduledDatetime
                                });
                            }
                        }

                        _context.PatientOrders.Add(order);
                        _context.CartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
                    }

                    _context.PatientCartOrders.RemoveRange(cartOrders);
                    i = _context.SaveChanges(true);
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
    }
}

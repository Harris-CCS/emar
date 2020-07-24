using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Model.Mappings;
using Emar.Core.Orders.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic.CompilerServices;

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
            patientId ??= resourceParameters.PatientId;

            var orders = _context.PatientCartOrders
                .Include(order => order.CartOrderAdministrations)
                .AsEnumerable();

            if ((patientId != null) &&
                (patientId != -1))
            {
                orders = orders
                    .Where(order => order.PatientId == patientId);
            }

            if (resourceParameters.OrderBy != null)
            {
                //get property mapping dictionary
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<CartOrderDto, PatientCartOrder>();

                orders = orders.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientCartOrder>.Create(orders.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public PatientCartOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            return _context.PatientCartOrders
                    .Include(order => order.CartOrderAdministrations)
                    .FirstOrDefault(order => order.Id == orderId);
        }

        public IEnumerable<CartOrderAdministration> GetAdministrations(long orderId)
        {
            return _context.PatientCartOrderAdministrations
                    .Where(administration => administration.PatientCartOrderId == orderId)
                    .AsEnumerable();
        }

        public CartOrderAdministration GetAdministration(long administrationId)
        {
            return _context.PatientCartOrderAdministrations
                    .FirstOrDefault(administration => administration.Id == administrationId);
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

                return _context.PatientCartOrders
                            .Include(order => order.CartOrderAdministrations)
                            .FirstOrDefault(order => order.Id == cartOrder.Id);
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
                        _context.PatientCartOrderAdministrations.Remove(administration);
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

            //var order = _context.PatientCartOrders.First(g => g.Id == cartOrder.Id);
            //_context.Entry(order).CurrentValues.SetValues(cartOrder);
            //_context.SaveChanges();


            ////using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            ////{
            ////    try
            ////    {
            ////        _context.PatientCartOrders.Update(cartOrder);
            ////        i = _context.SaveChanges();
            ////        transaction.Commit();
            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        i = 0;
            ////        transaction.Rollback();
            ////    }
            ////}

            return i > 0;
        }
        //public bool UpdateCartOrder(long? cartOrderId, CartOrderDto cartOrderDto, CartOrderDto cartOrderUpdateDto)
        //{
        //    int i = 0;

        //    using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var cartOrder = _context.PatientCartOrders
        //                                .Include(order => order.CartOrderAdministrations)
        //                                .FirstOrDefault(order => order.Id == cartOrderId);

        //            if (cartOrder != null)
        //            {
        //                cartOrder = CartOrderMapper.MapCartOrderDto(cartOrderUpdateDto);

        //                //if (cartOrder.CartOrderAdministrations != null)
        //                //{
        //                //    _context.PatientCartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);

        //                //    foreach (var cartAdministration in cartOrder.CartOrderAdministrations)
        //                //    {
        //                //        _context.PatientCartOrderAdministrations.Add(cartAdministration);
        //                //    }
        //                //}

        //                _context.PatientCartOrders.Update(cartOrder);
        //                i = _context.SaveChanges();
        //                transaction.Commit();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            i = 0;
        //            transaction.Rollback();
        //        }
        //    }

        //    return i > 0;
        //}

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
                        _context.PatientCartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
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
                            _context.PatientCartOrderAdministrations.RemoveRange(cartOrder.CartOrderAdministrations);
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
                            DoseUnit = cartOrder.DoseUnit,
                            MedicationRouteId = cartOrder.MedicationRouteId,
                            Priority = cartOrder.Priority,
                            FrequencyId = cartOrder.FrequencyId,
                            Prn = cartOrder.Prn,
                            PointInTime = cartOrder.PointInTime,
                            OrderStatus = OrderStatuses.Pending.ToString(),
                            BeginDatetime = cartOrder.BeginDatetime,
                            EndDatetime = cartOrder.EndDatetime,
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
    }
}

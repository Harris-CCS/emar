using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Carts.Model;
using Emar.Core.Helpers;
using Emar.Core.Orders.Model;
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
            return _context.PatientCartOrderAdministrations
                    .Where(administration => administration.PatientCartOrderId == orderId)
                    .AsEnumerable();
        }

        public CartOrderAdministration GetAdministration(long administrationId)
        {
            return _context.PatientCartOrderAdministrations
                    .FirstOrDefault(administration => administration.Id == administrationId);
        }
    }
}

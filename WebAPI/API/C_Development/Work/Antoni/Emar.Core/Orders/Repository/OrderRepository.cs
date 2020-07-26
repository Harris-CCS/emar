using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Orders.Model;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.Orders.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EmarContext _context;
        private readonly IPropertyMappingService _propertyMappingService;

        public OrderRepository()
        {

        }

        public OrderRepository(EmarContext emarContext, IPropertyMappingService propertyMappingService)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public PagedList<PatientOrder> GetOrders(long? patientId, OrdersResourceParameters resourceParameters)
        {
            patientId ??= resourceParameters.PatientId;

            var orders = _context.PatientOrders
                .Include(order => order.OrderEvents)
                .Include(order => order.OrderAdministrations)
                    .ThenInclude(administration => administration.OrderEvents)
                .Include(order => order.MedicationRoute)
                .Include(order => order.AddUser)
                .Include(order => order.OrderPhysicianUser)
                .Include(order => order.MedicationRoute)
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
                var propertyMappingDictionary = _propertyMappingService.GetPropertyMapping<PatientOrderDto, PatientOrder>();

                orders = orders.AsQueryable().ApplySort(resourceParameters.OrderBy, propertyMappingDictionary);
            }

            return PagedList<PatientOrder>.Create(orders.AsQueryable(), resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public PatientOrder GetOrder(long orderId, OrdersResourceParameters resourceParameters)
        {
            return _context.PatientOrders
                    .Include(order => order.OrderEvents)
                    .Include(order => order.OrderAdministrations)
                        .ThenInclude(administration => administration.OrderEvents)
                    .Include(order => order.MedicationRoute)
                    .Include(order => order.AddUser)
                    .Include(order => order.OrderPhysicianUser)
                    .FirstOrDefault(order => order.Id == orderId);
        }

        public IEnumerable<OrderAdministration> GetAdministrations(long orderId)
        {
            return _context.PatientOrderAdministrations
                    .Where(administration => administration.PatientOrderId == orderId)
                    .Include(administration => administration.OrderEvents)
                    .AsEnumerable();
        }

        public OrderAdministration GetAdministration(long administrationId)
        {
            return _context.PatientOrderAdministrations
                    .Include(administration => administration.OrderEvents)
                    .FirstOrDefault(administration => administration.Id == administrationId);
        }

        public IEnumerable<OrderEvent> GetEvents(long orderId)
        {
            return _context.PatientOrderEvents
                    .Where(@event => @event.PatientOrderId == orderId)
                    .AsEnumerable();
        }

        public OrderEvent GetEvent(long eventId)
        {
            return _context.PatientOrderEvents.Find(eventId);
        }

        public IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId)
        {
            return _context.PatientOrderEvents
                    .Where(@event => @event.OrderAdministrationId == administrationId)
                    .AsEnumerable();
        }
    }
}

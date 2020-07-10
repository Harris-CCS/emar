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

            var orders = _context.Orders
                .Include(order => order.Events)
                .Include(order => order.Administrations)
                    .ThenInclude(administration => administration.Events)
                .AsEnumerable();

            if (patientId != null)
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
            return _context.Orders
                    .Include(order => order.Events)
                    .Include(order => order.Administrations)
                        .ThenInclude(administration => administration.Events)
                    .FirstOrDefault(order => order.Id == orderId);
        }

        public IEnumerable<OrderAdministration> GetAdministrations(long orderId)
        {
            return _context.OrderAdministrations
                .Where(administration => administration.OrderId == orderId)
                .Include(administration => administration.Events)
                .AsEnumerable();
        }

        public OrderAdministration GetAdministration(long administrationId)
        {
            return _context.OrderAdministrations
                    .Include(administration => administration.Events)
                    .FirstOrDefault(administration => administration.Id == administrationId);
        }

        public IEnumerable<OrderEvent> GetEvents(long orderId)
        {
            return _context.OrderEvents
                .Where(@event => @event.OrderId == orderId)
                .AsEnumerable();
        }

        public OrderEvent GetEvent(long eventId)
        {
            return _context.OrderEvents.Find(eventId);
        }

        public IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId)
        {
            return _context.OrderEvents
                .Where(@event => @event.AdministrationId == administrationId)
                .AsEnumerable();
        }
    }
}

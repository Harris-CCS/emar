using System.Collections.Generic;
using System.Linq;
using Emar.Data;
using Emar.Data.Entities;

namespace Emar.Core.Orders.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EmarContext _context;

        public OrderRepository()
        {

        }

        public OrderRepository(EmarContext emarContext)
        {
            _context = emarContext;
        }

        public IEnumerable<Order> GetOrders(long? patientId, ResourceParameters resourceParameters)
        {
            var orders = _context.Orders.ToList();

            foreach (var order in orders)
            {
                order.Events = GetEvents(order.Id).ToList();

                if (resourceParameters.IncludeAdministrations)
                {
                    order.Administrations = GetAdministrations(order.Id).ToList();
                }
            }

            return orders.AsEnumerable();
        }

        public Order GetOrder(long orderId, ResourceParameters resourceParameters)
        {
            var order = _context.Orders.Find(orderId);

            if (order != null)
            {
                order.Events = GetEvents(orderId).ToList();

                if (resourceParameters.IncludeAdministrations)
                {
                    order.Administrations = GetAdministrations(orderId).ToList();
                }
            }

            return order;
        }

        public IEnumerable<OrderAdministration> GetAdministrations(long orderId)
        {
            var administrations = _context.OrderAdministrations.AsQueryable().Where(administration => administration.OrderId.Equals(orderId)).ToList();

            foreach (var administration in administrations)
            {
                administration.Events = GetAdministrationEvents(administration.Id).ToList();
            }

            return administrations.AsEnumerable();
        }

        public OrderAdministration GetAdministration(long administrationId)
        {
            var administration = _context.OrderAdministrations.Find(administrationId);
            administration.Events = GetAdministrationEvents(administrationId).ToList();

            return administration;
        }

        public IEnumerable<OrderEvent> GetEvents(long orderId)
        {
            var events = _context.OrderEvents.AsQueryable().Where(@event => @event.OrderId.Equals(orderId)).ToList();

            return events.AsEnumerable();
        }

        public OrderEvent GetEvent(long eventId)
        {
            var @event = _context.OrderEvents.Find(eventId);

            return @event;
        }

        public IEnumerable<OrderEvent> GetAdministrationEvents(long administrationId)
        {
            var events = _context.OrderEvents.AsQueryable().Where(@event => @event.AdministrationId.Equals(administrationId)).ToList();

            return events.AsEnumerable();
        }
    }
}

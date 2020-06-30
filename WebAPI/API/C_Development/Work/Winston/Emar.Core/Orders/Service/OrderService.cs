using System.Collections.Generic;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Model.Mappings;
using Emar.Core.Orders.Repository;

namespace Emar.Core.Orders.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public IEnumerable<OrderDto> GetOrders(long? patientId, ResourceParameters resourceParameters)
        {
            var entityOrders = _orderRepository.GetOrders(patientId, resourceParameters);
            var ordersList = new List<OrderDto>();

            foreach (var order in entityOrders)
            {
                ordersList.Add(OrderMapper.MapOrder(order));
            }

            return ordersList;
        }

        public OrderDto GetOrder(long orderId, ResourceParameters resourceParameters)
        {
            var entityOrder = _orderRepository.GetOrder(orderId, resourceParameters);
            var orderDto = OrderMapper.MapOrder(entityOrder);

            return orderDto;
        }

        public IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId)
        {
            var entityAdministrations = _orderRepository.GetAdministrations(orderId);
            var administrationsList = new List<OrderAdministrationDto>();

            foreach (var administration in entityAdministrations)
            {
                administrationsList.Add(OrderMapper.MapOrderAdministration(administration));
            }

            return administrationsList;
        }

        public OrderAdministrationDto GetAdministration(long administrationId)
        {
            var entityAdministration = _orderRepository.GetAdministration(administrationId);
            var administrationDto = OrderMapper.MapOrderAdministration(entityAdministration);

            return administrationDto;
        }

        public IEnumerable<OrderEventDto> GetEvents(long orderId)
        {
            var entityEvents = _orderRepository.GetEvents(orderId);
            var eventsList = new List<OrderEventDto>();

            foreach (var @event in entityEvents)
            {
                eventsList.Add(OrderMapper.MapOrderEvent(@event));
            }

            return eventsList;
        }

        public OrderEventDto GetEvent(long eventId)
        {
            var entityEvent = _orderRepository.GetEvent(eventId);
            var eventDto = OrderMapper.MapOrderEvent(entityEvent);

            return eventDto;
        }

        public IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId)
        {
            var entityEvents = _orderRepository.GetAdministrationEvents(administrationId);
            var eventsList = new List<OrderEventDto>();

            foreach (var @event in entityEvents)
            {
                eventsList.Add(OrderMapper.MapOrderEvent(@event));
            }

            return eventsList;
        }
    }
}

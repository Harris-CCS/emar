using System.Collections.Generic;
using System.Linq;
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

        public PagedList<PatientOrderDto> GetOrders(long? patientId, ResourceParameters resourceParameters)
        {
            var orders = _orderRepository.GetOrders(patientId, resourceParameters);

            if ((orders == null) ||
                (!orders.Any()))
            {
                return null;
            }

            var ordersList = new List<PatientOrderDto>();

            foreach (var order in orders)
            {
                ordersList.Add(OrderMapper.MapOrder(order));
            }

            return new PagedList<PatientOrderDto>(ordersList, orders.TotalCount, orders.CurrentPage, orders.PageSize);
        }

        public PatientOrderDto GetOrder(long orderId, ResourceParameters resourceParameters)
        {
            var order = _orderRepository.GetOrder(orderId, resourceParameters);

            if (order == null)
            {
                return null;
            }

            var orderDto = OrderMapper.MapOrder(order);

            return orderDto;
        }

        public IEnumerable<OrderAdministrationDto> GetAdministrations(long orderId)
        {
            var administrations = _orderRepository.GetAdministrations(orderId);
            var administrationsList = new List<OrderAdministrationDto>();

            foreach (var administration in administrations)
            {
                administrationsList.Add(OrderMapper.MapOrderAdministration(administration));
            }

            return administrationsList;
        }

        public OrderAdministrationDto GetAdministration(long administrationId)
        {
            var administration = _orderRepository.GetAdministration(administrationId);
            var administrationDto = OrderMapper.MapOrderAdministration(administration);

            return administrationDto;
        }

        public IEnumerable<OrderEventDto> GetEvents(long orderId)
        {
            var events = _orderRepository.GetEvents(orderId);
            var eventsList = new List<OrderEventDto>();

            foreach (var @event in events)
            {
                eventsList.Add(OrderMapper.MapOrderEvent(@event));
            }

            return eventsList;
        }

        public OrderEventDto GetEvent(long eventId)
        {
            var @event = _orderRepository.GetEvent(eventId);
            var eventDto = OrderMapper.MapOrderEvent(@event);

            return eventDto;
        }

        public IEnumerable<OrderEventDto> GetAdministrationEvents(long administrationId)
        {
            var events = _orderRepository.GetAdministrationEvents(administrationId);
            var eventsList = new List<OrderEventDto>();

            foreach (var @event in events)
            {
                eventsList.Add(OrderMapper.MapOrderEvent(@event));
            }

            return eventsList;
        }
    }
}

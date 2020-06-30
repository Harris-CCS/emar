using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        [HttpGet(Name = nameof(GetOrders))]
        [Produces(typeof(IEnumerable<OrderDto>))]
        public IActionResult GetOrders([FromQuery] ResourceParameters resourceParameters)
        {
            var orders = _orderService.GetOrders(null, resourceParameters);

            if (orders == null)
            {
                return NotFound($"No orders found");
            }

            return Ok(orders);
        }

        [HttpGet("{orderId}", Name = nameof(GetOrder))]
        [Produces(typeof(OrderDto))]
        public IActionResult GetOrder(long orderId, [FromQuery] ResourceParameters resourceParameters)
        {
            var order = _orderService.GetOrder(orderId, resourceParameters);

            if (order == null)
            {
                return NotFound($"Patient order with id {orderId} was not found");
            }

            return Ok(order);
        }

        [HttpGet("{orderId}/administrations", Name = nameof(GetAdministrations))]
        [Produces(typeof(IEnumerable<OrderAdministrationDto>))]
        public IActionResult GetAdministrations(int orderId)
        {
            var administrations = _orderService.GetAdministrations(orderId);

            if (administrations == null)
            {
                return NotFound($"Patient order administrations for patient order with id {orderId} were not found");
            }

            return Ok(administrations);
        }

        [HttpGet("{orderId}/administrations/{administrationId}", Name = nameof(GetAdministration))]
        [Produces(typeof(OrderAdministrationDto))]
        public IActionResult GetAdministration(int orderId, int administrationId)
        {
            var administration = _orderService.GetAdministration(administrationId);

            if (administration == null)
            {
                return NotFound($"Patient order administration with id {administrationId} was not found");
            }

            if (!administration.OrderId.Equals(orderId))
            {
                return NotFound($"Patient order administration with id {administrationId} is not part of patient order with id {orderId}");
            }

            return Ok(administration);
        }

        [HttpGet("{orderId}/administrations/{administrationId}/events", Name = nameof(GetAdministrationEvents))]
        [Produces(typeof(IEnumerable<OrderEventDto>))]
        public IActionResult GetAdministrationEvents(int administrationId)
        {
            var events = _orderService.GetAdministrationEvents(administrationId);

            if (events == null)
            {
                return NotFound($"Patient order administration events for order administration with id {administrationId} were not found");
            }

            return Ok(events);
        }

        [HttpGet("{orderId}/events", Name = nameof(GetEvents))]
        [Produces(typeof(IEnumerable<OrderEventDto>))]
        public IActionResult GetEvents(int orderId, [FromQuery] ResourceParameters resourceParameters)
        {
            var events = _orderService.GetEvents(orderId);

            if (events == null)
            {
                return NotFound($"Patient order events for order with id {orderId} were not found");
            }

            if ((resourceParameters != null) &&
                (resourceParameters.IncludeAdministrationsEvents == false))
            {
                events = events.Where(@event => @event.AdministrationId != null);
            }

            return Ok(events.OrderBy(@event => @event.SystemDateTime));
        }

        [HttpGet("{orderId}/events/{eventId}", Name = nameof(GetEvent))]
        [Produces(typeof(OrderEventDto))]
        public IActionResult GetEvent(int orderId, int eventId)
        {
            var @event = _orderService.GetEvent(eventId);

            if (@event == null)
            {
                return NotFound($"Patient order event with id {eventId} was not found");
            }

            if (!@event.OrderId.Equals(orderId))
            {
                return NotFound($"Patient order event with id {eventId} is not part of patient order with id {orderId}");
            }

            return Ok(@event);
        }
    }
}
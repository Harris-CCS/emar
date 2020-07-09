using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using Emar.Core;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Emar.Data.Entities;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public OrdersController(IOrderService orderService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = nameof(GetOrders))]
        [HttpHead]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetOrders([FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<PatientOrderDto, Order>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientOrderDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            PagedList<PatientOrderDto> orders = _orderService.GetOrders(null, resourceParameters);

            if (orders == null) { return NotFound($"No orders found"); }

            var paginationMetadata = new
            {
                totalCount = orders.TotalCount,
                pageSize = orders.PageSize,
                currentPage = orders.CurrentPage,
                totalPages = orders.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            if (parsedMediaType.MediaType.Equals(MediaTypes.PcEmar))
            {
                var links = CreateHateOasLinksForOrders(resourceParameters, orders.HasNext, orders.HasPrevious);
                var shapedOrders = ((IEnumerable<PatientOrderDto>)orders).ShapeData(resourceParameters.Fields);

                var shapedOrdersWithLinks = shapedOrders.Select(order =>
                {
                    var orderAsDictionary = order as IDictionary<string, object>;
                    var orderLinks = CreateHateOasLinksForOrder((long)orderAsDictionary["Id"], resourceParameters);

                    orderAsDictionary.Add("links", orderLinks);

                    return orderAsDictionary;
                });

                var linkedOrderResource = new
                {
                    orders = shapedOrdersWithLinks,
                    links
                };

                return Ok(linkedOrderResource);
            }

            return Ok(((IEnumerable<PatientOrderDto>)orders).ShapeData(resourceParameters.Fields));
        }

        [HttpGet("{orderId}", Name = nameof(GetOrder))]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetOrder(long orderId, [FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientOrderDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var order = _orderService.GetOrder(orderId, resourceParameters);

            if (order == null) { return NotFound($"Patient order with id {orderId} was not found"); }

            if (parsedMediaType.MediaType.Equals(MediaTypes.PcEmar))
            {
                var links = CreateHateOasLinksForOrder(orderId, resourceParameters);
                var linkedResourceToReturn = order.ShapeData(resourceParameters.Fields) as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return Ok(linkedResourceToReturn);
            }

            return Ok(order.ShapeData(resourceParameters.Fields));
        }

        [HttpGet("{orderId}/administrations", Name = nameof(GetAdministrations))]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetAdministrations(int orderId, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            var administrations = _orderService.GetAdministrations(orderId);

            if (administrations == null)
            {
                return NotFound($"Patient order administrations for patient order with id {orderId} were not found");
            }

            return Ok(administrations);
        }

        [HttpGet("{orderId}/administrations/{administrationId}", Name = nameof(GetAdministration))]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetAdministration(int orderId, int administrationId, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

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
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetAdministrationEvents(int administrationId, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            var events = _orderService.GetAdministrationEvents(administrationId);

            if (events == null)
            {
                return NotFound($"Patient order administration events for order administration with id {administrationId} were not found");
            }

            return Ok(events);
        }

        [HttpGet("{orderId}/events", Name = nameof(GetEvents))]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetEvents(int orderId, [FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

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
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetEvent(int orderId, int eventId, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

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

        private string CreateOrdersResourceUri(ResourceParameters resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        resourceParameters.PageNumber = resourceParameters.PageNumber - 1;
                        return Url.Link(nameof(GetOrders), resourceParameters);
                    }
                case ResourceUriType.NextPage:
                    {
                        resourceParameters.PageNumber = resourceParameters.PageNumber + 1;
                        return Url.Link(nameof(GetOrders), resourceParameters);
                    }
                case ResourceUriType.Current:
                default:
                    {
                        return Url.Link(nameof(GetOrders), resourceParameters);
                    }
            }
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrder(long? orderId, [FromQuery] ResourceParameters resourceParameters)
        {
            List<HateOasLinkDto> links = new List<HateOasLinkDto>();

            if (String.IsNullOrWhiteSpace(resourceParameters.Fields))
            {
                links.Add(
                    new HateOasLinkDto(Url.Link(nameof(GetOrder), new { orderId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new HateOasLinkDto(Url.Link(nameof(GetOrder), new { orderId, resourceParameters.Fields }),
                    "self",
                    "GET"));
            }

            //links.Add(
            //    new HateOasLinkDto(Url.Link(nameof(CreateOrder), new { patientId }),
            //    "create_order",
            //    "POST"));

            return links;
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrders([FromQuery] ResourceParameters resourceParameters, bool hasNext, bool hasPrevious)
        {
            List<HateOasLinkDto> links = new List<HateOasLinkDto>();

            links.Add(
                new HateOasLinkDto(CreateOrdersResourceUri(resourceParameters, ResourceUriType.Current),
                "self",
                "GET"));

            if (hasNext)
            {
                links.Add(
                    new HateOasLinkDto(CreateOrdersResourceUri(resourceParameters, ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new HateOasLinkDto(CreateOrdersResourceUri(resourceParameters, ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }

            return links;
        }
    }
}
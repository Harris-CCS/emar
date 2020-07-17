using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Patient Orders Controller
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="orderService"></param>
        /// <param name="propertyMappingService"></param>
        /// <param name="propertyCheckerService"></param>
        public OrdersController(IOrderService orderService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        /// <summary>
        /// Get a list of orders in the system.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="patientId">
        /// *Optional.* \
        /// eMAR unique patient identifier.  If it is passed in then this API call will return all the orders for this patient.
        /// </param>
        /// <param name="orderBy">
        /// *Optional.* \
        /// Comma delimited list Patient Order element to sort by:
        /// * **Id**
        /// * **Priority**
        /// * **OrderStatus**
        /// * **Begin** or **BeginDate** or **BeginTime**
        /// \
        /// \
        /// Optional **ASC** or **DESC** commands can be suffixed. \
        /// *Default:* **Id ASC**
        /// </param>
        /// <param name="fields">
        /// *Optional.* \
        /// Comma delimited list of order elements to be returned.  If omitted, all order elements are returned.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet(Name = nameof(GetOrders))]
        [HttpHead]
        public ActionResult<IEnumerable<PatientOrderDto>> GetOrders(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string patientId,
            [FromQuery] string orderBy,
            [FromQuery] string fields
            )
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            OrdersResourceParameters resourceParameters = new OrdersResourceParameters
            {
                PatientId = long.TryParse(patientId, out long PtId) ? PtId : -1,
                OrderBy = orderBy,
                Fields = fields
            };

            if (!_propertyMappingService.ValidMappingExistsFor<PatientOrderDto, PatientOrder>(orderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientOrderDto>(fields))
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
                var shapedOrders = ((IEnumerable<PatientOrderDto>)orders).ShapeData(fields);

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

            return Ok(((IEnumerable<PatientOrderDto>)orders).ShapeData(fields));
        }

        /// <summary>
        /// Get an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="fields">
        /// *Optional.* \
        /// Comma delimited list of order elements to be returned.  If omitted, all order elements are returned.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <returns>An ActionResult of type OrderDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}", Name = nameof(GetOrder))]
        public ActionResult<PatientOrderDto> GetOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string fields,
            long orderId
            )
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            OrdersResourceParameters resourceParameters = new OrdersResourceParameters
            {
                Fields = fields,
            };

            if (!_propertyCheckerService.TypeHasProperties<PatientOrderDto>(fields))
            {
                return BadRequest();
            }

            var order = _orderService.GetOrder(orderId, resourceParameters);

            if (order == null) { return NotFound($"Patient order with id {orderId} was not found"); }

            if (parsedMediaType.MediaType.Equals(MediaTypes.PcEmar))
            {
                var links = CreateHateOasLinksForOrder(orderId, resourceParameters);
                var linkedResourceToReturn = order.ShapeData(fields) as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return Ok(linkedResourceToReturn);
            }

            return Ok(order.ShapeData(fields));
        }

        /// <summary>
        /// Get the administrations of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderAdministrationDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/administrations", Name = nameof(GetAdministrations))]
        public ActionResult<IEnumerable<OrderAdministrationDto>> GetAdministrations(
            [FromHeader(Name = "Accept")] string mediaType,
            int orderId
            )
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

        /// <summary>
        /// Get an administration by administration id of an order by order id.
        /// </summary>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="administrationId">
        /// Unique order administration identifier.
        /// </param>
        /// <returns>An ActionResult of type OrderAdministrationDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/administrations/{administrationId}", Name = nameof(GetAdministration))]
        public ActionResult<OrderAdministrationDto> GetAdministration(
            [FromHeader(Name = "Accept")] string mediaType,
            int orderId,
            int administrationId
            )
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

        /// <summary>
        /// Get the events of an administration by administration id of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <param name="administrationId">
        /// Unique order administration identifier.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderEventDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/administrations/{administrationId}/events", Name = nameof(GetAdministrationEvents))]
        public ActionResult<IEnumerable<OrderEventDto>> GetAdministrationEvents(
            [FromHeader(Name = "Accept")] string mediaType,
            int orderId,
            int administrationId
            )
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

        /// <summary>
        /// Get the events of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <returns>An ActionResult of IEnumerable collection of type OrderEventDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/events", Name = nameof(GetEvents))]
        public ActionResult<IEnumerable<OrderEventDto>> GetEvents(
            [FromHeader(Name = "Accept")] string mediaType,
            int orderId
            )
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

            events = events.Where(@event => @event.AdministrationId != null);

            return Ok(events.OrderBy(@event => @event.SystemDateTime));
        }

        /// <summary>
        /// Get the event by event id of an order by order id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="orderId">
        /// Unique order identifier.
        /// </param>
        /// <param name="eventId">
        /// Unique order event identifier.
        /// </param>
        /// <returns>An ActionResult of type OrderEventDto</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("{orderId}/events/{eventId}", Name = nameof(GetEvent))]
        public ActionResult<OrderEventDto> GetEvent(
            [FromHeader(Name = "Accept")] string mediaType,
            int orderId,
            int eventId
            )
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

        private string CreateOrdersResourceUri(BaseResourceParameters resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        resourceParameters.PageNumber -= 1;
                        return Url.Link(nameof(GetOrders), resourceParameters);
                    }
                case ResourceUriType.NextPage:
                    {
                        resourceParameters.PageNumber += 1;
                        return Url.Link(nameof(GetOrders), resourceParameters);
                    }
                case ResourceUriType.Current:
                default:
                    {
                        return Url.Link(nameof(GetOrders), resourceParameters);
                    }
            }
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrder(long? orderId, [FromQuery] BaseResourceParameters resourceParameters)
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

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrders([FromQuery] BaseResourceParameters resourceParameters, bool hasNext, bool hasPrevious)
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
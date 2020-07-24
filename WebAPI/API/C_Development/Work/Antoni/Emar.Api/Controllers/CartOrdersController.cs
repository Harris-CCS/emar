using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Emar.Core;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Patient Cart Orders Controller
    /// </summary>
    [ApiController]
    [Route("api/carts")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class CartOrdersController : ControllerBase
    {
        private readonly ICartOrderService _cartOrderService;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cartOrderService"></param>
        /// <param name="propertyMappingService"></param>
        /// <param name="propertyCheckerService"></param>
        public CartOrdersController(ICartOrderService cartOrderService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _cartOrderService = cartOrderService ?? throw new ArgumentNullException(nameof(cartOrderService));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        /// <summary>
        /// Get all cart orders for a patient the user entered.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpGet("{patientId}", Name = nameof(GetCartOrders))]
        public ActionResult<IEnumerable<CartOrderDto>> GetCartOrders(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            long? patientId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (patientId == null)
            {
                return BadRequest("Patient id is missing.");
            }

            OrdersResourceParameters resourceParameters = new OrdersResourceParameters
            {
                UserId = userId,
                PatientId = patientId
            };

            PagedList<CartOrderDto> orders = _cartOrderService.GetOrders(patientId, resourceParameters);

            if (orders == null)
            {
                return NotFound($"No cart orders found on patient with id '{patientId}' from user with id '{userId}'.");
            }

            var paginationMetadata = new
            {
                totalCount = orders.TotalCount,
                pageSize = orders.PageSize,
                currentPage = orders.CurrentPage,
                totalPages = orders.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateHateOasLinksForOrders(resourceParameters, orders.HasNext, orders.HasPrevious);
            var shapedOrders = ((IEnumerable<CartOrderDto>)orders).ShapeData(null);
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

        /// <summary>
        /// Get a cart order for a patient the user entered.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="cartOrderId">
        /// Unique cart order identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpGet("orders/{cartOrderId}", Name = nameof(GetCartOrder))]
        public ActionResult<CartOrderDto> GetCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            [FromRoute(Name = "cartOrderId")] long cartOrderId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (cartOrderId == null)
            {
                return BadRequest("Cart order id is missing.");
            }

            OrdersResourceParameters resourceParameters = new OrdersResourceParameters
            {

            };

            var order = _cartOrderService.GetOrder(cartOrderId, resourceParameters);

            if (order == null)
            {
                return NotFound($"Patient cart order with id '{cartOrderId}' was not found.");
            }

            var links = CreateHateOasLinksForOrder(cartOrderId, resourceParameters);
            var linkedResourceToReturn = order.ShapeData("") as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        /// <summary>
        /// Add a cart order for a patient the user entered.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <param name="cartOrderBody">
        /// Body of the order to be added in the cart.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        /// <remarks>
        /// The JSON in the request body can **NOT** have Order and Administrations Ids set **OR** they **MUST** be set to 0.
        /// </remarks>
        [HttpPost("{patientId}", Name = nameof(AddCartOrder))]
        public ActionResult<CartOrderDto> AddCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            [FromBody] CartOrderDto cartOrderBody,
            long? patientId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (patientId == null)
            {
                return BadRequest("Patient id is missing.");
            }

            ///  cartOrderAddDto can NOT have Order & Administrations Ids set OR they MUST be set to 0 
            var order = _cartOrderService.AddCartOrder(cartOrderBody);

            if (order == null)
            {
                return NotFound($"New cart order for patient with id '{patientId}' from user with id '{userId}' was not added.");
            }

            return Ok(order);
        }

        /// <summary>
        /// Update a cart order.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="cartOrderId">
        /// Unique cart order identifier.
        /// </param>
        /// <param name="cartOrderBody">
        /// Body of the order to be added in the cart.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpPut("orders/{cartOrderId}", Name = nameof(UpdateCartOrder))]
        public IActionResult UpdateCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            [FromBody] CartOrderDto cartOrderBody,
            long cartOrderId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (cartOrderId == null)
            {
                return BadRequest("Cart order id is missing.");
            }

            var cartOrderDto = _cartOrderService.GetOrder(cartOrderId, null);

            if (cartOrderDto == null)
            {
                return NotFound($"Patient cart order with id '{cartOrderId}' was not found.");
            }

            if (_cartOrderService.UpdateCartOrder(cartOrderId, cartOrderDto, cartOrderBody))
            {
                var order = _cartOrderService.GetOrder(cartOrderId, new OrdersResourceParameters());
                var links = CreateHateOasLinksForOrder(cartOrderId, new OrdersResourceParameters());
                var linkedResourceToReturn = order.ShapeData("") as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return Ok(linkedResourceToReturn);
            }

            return BadRequest($"Cart order with id '{cartOrderId}' was not updated successfully.");
        }

        /// <summary>
        /// Delete a cart order.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="cartOrderId">
        /// Unique cart order identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpDelete("orders/{cartOrderId}", Name = nameof(DeleteCartOrder))]
        public ActionResult DeleteCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            long? cartOrderId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (cartOrderId == null)
            {
                return BadRequest("Cart order id is missing.");
            }

            if (_cartOrderService.DeleteCartOrder(cartOrderId))
            {
                return NoContent();
            }

            return BadRequest($"Cart order with id '{cartOrderId}' was not deleted successfully.");
        }

        /// <summary>
        /// Delete the cart.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpDelete("{patientId}", Name = nameof(DeleteCart))]
        public ActionResult DeleteCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            long? patientId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (patientId == null)
            {
                return BadRequest("Patient id is missing.");
            }

            if (_cartOrderService.DeleteCartOrders(userId, patientId))
            {
                return NoContent();
            }

            return BadRequest($"Cart orders on patient with id '{patientId}' from user with id '{userId}' were not deleted successfully.");
        }

        /// <summary>
        /// Order (checkout) the cart orders.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from Accept header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <returns></returns>
        [HttpPost("{patientId}/checkout", Name = nameof(CheckoutCart))]
        public ActionResult CheckoutCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "X-User")] int? userId,
            long? patientId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            if (patientId == null)
            {
                return BadRequest("Patient id is missing.");
            }

            if (_cartOrderService.CheckoutOrders(userId, patientId))
            {
                return NoContent();
            }

            return BadRequest($"Cart orders on patient with id '{patientId}' from user with id '{userId}' were not signed successfully.");
        }

        private string CreateOrdersResourceUri(BaseResourceParameters resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        resourceParameters.PageNumber -= 1;
                        return Url.Link(nameof(GetCartOrders), resourceParameters);
                    }
                case ResourceUriType.NextPage:
                    {
                        resourceParameters.PageNumber += 1;
                        return Url.Link(nameof(GetCartOrders), resourceParameters);
                    }
                case ResourceUriType.Current:
                default:
                    {
                        return Url.Link(nameof(GetCartOrders), resourceParameters);
                    }
            }
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrder(long? cartOrderId, [FromQuery] BaseResourceParameters resourceParameters)
        {
            List<HateOasLinkDto> links = new List<HateOasLinkDto>();

            if (String.IsNullOrWhiteSpace(resourceParameters.Fields))
            {
                links.Add(
                    new HateOasLinkDto(Url.Link(nameof(GetCartOrder), new { }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new HateOasLinkDto(Url.Link(nameof(GetCartOrder), new { resourceParameters.Fields }),
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
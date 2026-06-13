using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Emar.Api.Helpers;
using Emar.Core.Carts.Model;
using Emar.Core.Carts.Service;
using Emar.Core.Helpers;
using Emar.Core.Orders.Service;
using Emar.Core.ResourceParameters;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Http;
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
        private readonly IOrderService _orderService;
        //private readonly IPropertyMappingService _propertyMappingService;
        //private readonly IPropertyCheckerService _propertyCheckerService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cartOrderService"></param>
        public CartOrdersController(ICartOrderService cartOrderService, IOrderService orderService/*,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService*/)
        {
            _cartOrderService = cartOrderService ?? throw new ArgumentNullException(nameof(cartOrderService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            //_propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            //_propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        /// <summary>
        /// Get all cart orders for a patient the user entered.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from EMAR-User header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <param name="pageResource">
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpGet("{patientId}", Name = nameof(GetCartOrders))]
        [ProducesResponseType(typeof(IEnumerable<CartOrderDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<CartOrderDto>> GetCartOrders(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId,
            [FromQuery] PageResource pageResource
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

            var resource = new BaseLinkResource
            {
                UserId = userId.Value,
                PatientId = patientId,
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            var orders = _cartOrderService.GetOrders(resource);

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

            Response.Headers.Add("EMAR-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateHateOasLinksForOrders(pageResource, orders.HasNext, orders.HasPrevious);
            var shapedOrders = ((IEnumerable<CartOrderDto>)orders).ShapeData(null);
            var shapedOrdersWithLinks = shapedOrders.Select(order =>
            {
                var orderAsDictionary = order as IDictionary<string, object>;
                var orderLinks = CreateHateOasLinksForOrder((long)orderAsDictionary["Id"]);

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
        /// Unique user identifier from EMAR-User header.
        /// </param>
        /// <param name="cartOrderId">
        /// Unique cart order identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpGet("orders/{cartOrderId}", Name = nameof(GetCartOrder))]
        [ProducesResponseType(typeof(ActionResult<CartOrderDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<CartOrderDto> GetCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
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

            var resource = new BaseLinkResource
            {
                UserId = userId.Value,
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            var order = _cartOrderService.GetOrder(cartOrderId, resource);

            if (order == null)
            {
                return NotFound($"Patient cart order with id '{cartOrderId}' was not found.");
            }

            var links = CreateHateOasLinksForOrder(cartOrderId);
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
        /// Unique user identifier from EMAR-User header.
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
        [ProducesResponseType(201)] // (created) - if a new resource is created, contain an entity which describes the status of the request and refers to the new resource, and a Location header.
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        //[ProducesResponseType(412)] // (precondition failed) e.g. conflict by performing conditional update
        [ProducesResponseType(415)] // (unsupported media type) - received representation is not supported
        public ActionResult<CartOrderDto> AddCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId,
            [FromBody] CartOrderIuDto cartOrderBody
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

            var resource = new BaseLinkResource
            {
                UserId = userId.Value,
                PatientId = patientId,
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            //  cartOrderAddDto can NOT have Order and Administrations Ids set OR they MUST be set to 0 
            var order = _cartOrderService.AddCartOrder(cartOrderBody, resource);

            if (order == null)
            {
                return NotFound($"New cart order for patient with id '{patientId}' from user with id '{userId}' was not added.");
            }

            var links = CreateHateOasLinksForOrder(order.Id);
            var linkedResourceToReturn = order.ShapeData("") as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            Response.Headers.Add("EMAR-User", userId.ToString());

            return new JsonResult(linkedResourceToReturn)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// Update a cart order.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from EMAR-User header.
        /// </param>
        /// <param name="cartOrderId">
        /// Unique cart order identifier.
        /// </param>
        /// <param name="cartOrderBody">
        /// Body of the order to be added in the cart.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpPut("orders/{cartOrderId}", Name = nameof(UpdateCartOrder))]
        [ProducesResponseType(typeof(IActionResult), 200)] // (OK) - if an existing resource has been updated
        [ProducesResponseType(201)] // (created) - if a new resource is created
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(415)] // (unsupported media type) - received representation is not supported
        public IActionResult UpdateCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "cartOrderId")] long cartOrderId,
            [FromBody] CartOrderIuDto cartOrderBody
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

            var resource = new BaseLinkResource
            {
                UserId = userId.Value,
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            var cartOrderDto = _cartOrderService.GetOrder(cartOrderId, resource);

            if (cartOrderDto == null)
            {
                return NotFound($"Patient cart order with id '{cartOrderId}' was not found.");
            }

            if (_cartOrderService.UpdateCartOrder(cartOrderBody))
            {
                var order = _cartOrderService.GetOrder(cartOrderId, resource);
                var links = CreateHateOasLinksForOrder(cartOrderId);
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
        /// <param name="cartOrderId">
        /// Unique cart order identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpDelete("orders/{cartOrderId}", Name = nameof(DeleteCartOrder))]
        //[ProducesResponseType(200)] // (OK) - the resource has been deleted
        [ProducesResponseType(204)] // (NoContent) - the resource has been deleted
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        //[ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(410)] // (gone)
        public ActionResult DeleteCartOrder(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "cartOrderId")] long cartOrderId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
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
        /// Unique user identifier from EMAR-User header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <returns>An ActionResult of type CartOrderDto</returns>
        [HttpDelete("{patientId}", Name = nameof(DeleteCart))]
        //[ProducesResponseType(200)] // (OK) - the resource has been deleted
        [ProducesResponseType(204)] // (NoContent) - the resource has been deleted
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        //[ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(410)] // (gone)
        public ActionResult DeleteCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId
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

            if (_cartOrderService.DeleteCartOrders(userId.Value, patientId))
            {
                return NoContent();
            }

            return BadRequest($"Cart orders on patient with id '{patientId}' from user with id '{userId}' were not deleted successfully.");
        }

        /// <summary>
        /// Get the lists of ordering physicians, cart orders with drug interactions and allergy reactions; along with the lists of override reasons for drug interactions and allergy reactions.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from EMAR-User header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <returns>An ActionResult of type OverrideCartOrdersFrameworkDto</returns>
        [HttpGet("{patientId}/precheckout", Name = nameof(GetCartPreCheckoutData))]
        [ProducesResponseType(typeof(ActionResult<CartOrderDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<CartPreCheckoutRequestDataDto> GetCartPreCheckoutData(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId
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

            var data = _cartOrderService.GetCartPreCheckoutData(userId.Value, patientId);

            if (data == null)
            {
                return NotFound($"No ordering physicians nor cart orders with drug interactions and/or allergy reactions found on patient with id '{patientId}' from user with id '{userId}'.");
            }

            return Ok(data);
        }

        /// <summary>
        /// Set the ordering physician and the drug interactions &amp; allergy reactions override rationalia.
        /// Order (checkout) the cart orders.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier from EMAR-User header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <param name="cartPreCheckoutResponseData">
        /// Body of pre-checkout response data (Ordering Physician Id, Drug Interaction Override Rationalia, Allergy Reaction Override Rationalia).
        /// </param>
        /// <returns></returns>
        [HttpPost("{patientId}/checkout", Name = nameof(CheckoutCart))]
        [ProducesResponseType(typeof(string), 201)] // (created) - if a new resource is created, contain an entity which describes the status of the request and refers to the new resource, and a Location header.
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        //[ProducesResponseType(412)] // (precondition failed) e.g. conflict by performing conditional update
        [ProducesResponseType(415)] // (unsupported media type) - received representation is not supported
        public ActionResult CheckoutCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId,
            [FromBody] CartPreCheckoutResponseDataDto cartPreCheckoutResponseData
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

            //1) Write a service method to call the existing GetCartOrders method, add the medicationId
            //      for each one into a list and return that list to the controller.
            //2) Change the CheckoutCart method in the CartOrderRepository to call the new service
            //      method and get the list of medicationIds for all medications in the cart prior to
            //      checking out the cart.
            //3) After checking out the cart, pass that list of medicationIds for the cart orders that
            //      we just checked out all the way down to where we get all patient orders and
            //      compare them against the current order.
            //4) In the service method, take the list of orders that we already have and filter it
            //      to make two new lists.
            //      A) The orders that were just checked out (where the medicationId is in the list of
            //          medicationIds we just checked out.
            //      B) The orders that were already on the patient before the user checked out the cart.
            //5) Pass the two lists of orders (as optional parameters that default to null) and the list
            //      of cart orders all the way down the stack to
            //      MedicationManager.AddInteractionsAndReactionsToMedications.
            //6) In that guy, if we have the lists, then use them.  If we don't have the list, then
            //      do the existing behavior of querying the DB.

            var resource = new BaseLinkResource
            {
                UserId = userId.Value,
                PatientId = patientId,
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 })
            };

            //Get the list of MedicationIds for the orders in the cart that we are checking out.
            var medicationIds = _cartOrderService.GetAllMedicationIdsInCart(resource);

            //Check out the cart.
            //1) Make patient orders for the cart orders.
            //2) Write stuff back into PulseCheck.
            //3) Generate pharmacy notifications if this is an "inpatient" patient.
            //4) Cleanup the cart orders.
            if (!_cartOrderService.CheckoutOrders(cartPreCheckoutResponseData, userId.Value, patientId))
            {
                return BadRequest($"Cart orders on patient with id '{patientId}' from user with id '{userId}' were not signed successfully.  Related ordering physician and (if applicable) override rationalia were not saved successfully.");
            }
            
            //Recalculate the interactions and reactions for all orders on this patient.
            //We'll need to pass medicationIds all the way down the stack here so that we can filter based on it.
            _orderService.UpdatePatientOrderInteractionsAndReactions(patientId, medicationIds);

            Response.Headers.Add("EMAR-User", userId.Value.ToString());
            return CreatedAtRoute(nameof(OrdersController.GetOrders), new { patientId }, $"Cart orders on patient with id '{patientId}' from user with id '{userId}' were signed successfully.  Related ordering physician and (if applicable) override rationalia were saved successfully.");
        }

        private string CreateOrdersResourceUri(PageResource pageResource, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        pageResource.PageNumber -= 1;
                        return Url.Link(nameof(GetCartOrders), pageResource);
                    }
                case ResourceUriType.NextPage:
                    {
                        pageResource.PageNumber += 1;
                        return Url.Link(nameof(GetCartOrders), pageResource);
                    }
                default:
                    {
                        return Url.Link(nameof(GetCartOrders), pageResource);
                    }
            }
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrder(long? cartOrderId)
        {
            return new List<HateOasLinkDto>
            {
                new HateOasLinkDto(Url.Link(nameof(GetCartOrder), new {cartOrderId}),
                    "get_cart_order",
                    "GET"),
                new HateOasLinkDto(Url.Link(nameof(UpdateCartOrder), new {cartOrderId}),
                    "update_cart_order",
                    "PUT"),
                new HateOasLinkDto(Url.Link(nameof(DeleteCartOrder), new {cartOrderId}),
                    "delete_cart_order",
                    "DELETE")
            };
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForOrders([FromQuery] PageResource pageResource, bool hasNext, bool hasPrevious)
        {
            var links = new List<HateOasLinkDto>
            {
                new HateOasLinkDto(CreateOrdersResourceUri(pageResource, ResourceUriType.Current),
                    "self",
                    "GET")
            };

            if (hasNext)
            {
                links.Add(
                    new HateOasLinkDto(CreateOrdersResourceUri(pageResource, ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new HateOasLinkDto(CreateOrdersResourceUri(pageResource, ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }

            return links;
        }
    }
}
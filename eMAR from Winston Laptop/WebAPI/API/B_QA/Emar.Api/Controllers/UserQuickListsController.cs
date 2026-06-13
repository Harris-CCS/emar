using System;
using System.Collections.Generic;
using Emar.Api.Helpers;
using Emar.Core.Carts.Model;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Emar.Core.ResourceParameters;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/userquicklists")]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class UserQuickListsController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public UserQuickListsController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        /// <summary>
        /// Return initial information about a User's Quick List:
        /// will return the name and contents of the first Quick List tab,
        /// and a list of all the tabs that will return remembered orders
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="siteId">(Optional) The Site to retrieve the user's quick list for (if omitted, return the user's quick list for all sites)</param>
        /// <param name="userId">The user to retrieve the quick list for (provided in the header)</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart.  Also, perform drug interaction checking against current patient orders, patient cart orders and patient home medications as well as allergy reaction checking.</param>
        /// <returns></returns>
        [HttpGet(Name = nameof(GetUserQuickListInitial))]
        [ProducesResponseType(typeof(UserQuickListFrameworkDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserQuickListFrameworkDto> GetUserQuickListInitial(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-Patient")] long? patientId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            var tabLinkBase = Url.Link(nameof(GetUserQuickListTab), new { tabTitle = "C" });
            tabLinkBase = tabLinkBase.Substring(0, tabLinkBase.LastIndexOf("/tabs/", StringComparison.InvariantCultureIgnoreCase) + 6);

            var resource = new BaseLinkResource
            {
                SiteId = siteId.Value,
                UserId = userId.Value,
                PatientId = patientId ?? 0,
                LinkGetUserQuickListTab = tabLinkBase,
                LinkCopyItemToCart = Url.Link(nameof(CopyQuickListItemToCart), new { quickListItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.UserQuickListItem, itemId = -99 })
            };

            var ret = _orderService.GetInitialUserQuickList(resource);

            if (ret == null)
                return NotFound($"User with id {userId} does not exist");

            //var links = CreateHateOasLinksForQuickListFramework(ret);

            return Ok(ret);
        }

        /// <summary>
        /// Return the contents for one tab of a User's Quick List
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="siteId">(Optional) The Site to retrieve the user's quick list for (if omitted, return the user's quick list for all sites)</param>
        /// <param name="userId">The user to retrieve the quick list for (provided in the header)</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart.  Also, perform drug interaction checking against current patient orders, patient cart orders and patient home medications as well as allergy reaction checking.</param>
        /// <param name="tabTitle">the tab to retrieve remembered orders for</param>
        /// <returns></returns>
        [HttpGet("tabs/{tabTitle}", Name = nameof(GetUserQuickListTab))]
        [ProducesResponseType(typeof(IEnumerable<UserQuickListItemDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<UserQuickListItemDto>> GetUserQuickListTab(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-Patient")] long? patientId,
            [FromRoute(Name = "tabTitle")] string tabTitle)
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            var resource = new BaseLinkResource
            {
                SiteId = siteId.Value,
                UserId = userId.Value,
                PatientId = patientId ?? 0,
                LinkCopyItemToCart = Url.Link(nameof(CopyQuickListItemToCart), new { quickListItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.UserQuickListItem, itemId = -99 })
            };

            var ret = _orderService.GetQuickListTab(tabTitle, resource);

            if (ret != null) return Ok(ret);

            return NotFound(siteId == null
                ? $"User with id {userId} does not have any Quick List Orders for the '{tabTitle}' tab"
                : $"User with id {userId} does not have any Quick List Orders for the '{tabTitle}' tab for Site {siteId}");
        }

        /// <summary>
        /// Return a User's Quick List item
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="siteId">(Optional) The Site to retrieve the user's quick list for (if omitted, return the user's quick list for all sites)</param>
        /// <param name="userId">The user to retrieve the quick list for (provided in the header)</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart.  Also, perform drug interaction checking against current patient orders, patient cart orders and patient home medications as well as allergy reaction checking.</param>
        /// <param name="quickListItemId"></param>
        /// <returns></returns>
        [HttpGet("{quickListItemId}", Name = nameof(GetQuickListItem))]
        [ProducesResponseType(typeof(UserQuickListItemDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserQuickListItemDto> GetQuickListItem(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-Patient")] long? patientId,
            [FromRoute(Name = "quickListItemId")] int quickListItemId)
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            var resource = new BaseLinkResource
            {
                SiteId = siteId.Value,
                UserId = userId.Value,
                PatientId = patientId ?? 0,
                LinkCopyItemToCart = Url.Link(nameof(CopyQuickListItemToCart), new { quickListItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.UserQuickListItem, itemId = -99 })
            };

            var ret = _orderService.GetQuickListItem(quickListItemId, resource);

            if (ret == null)
            {
                return NotFound($"User Quick List Order with id '{quickListItemId}' does not exist.");
            }

            return Ok(ret);
        }

        [HttpPost(Name = nameof(AddQuickListItem))]
        [ProducesResponseType(201)] // (created) - if a new resource is created, contain an entity which describes the status of the request and refers to the new resource, and a Location header.
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        //[ProducesResponseType(412)] // (precondition failed) e.g. conflict by performing conditional update
        [ProducesResponseType(415)] // (unsupported media type) - received representation is not supported
        public ActionResult<UserQuickListItemDto> AddQuickListItem(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromBody] UserQuickListItemAddDto quickListItemBody
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

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            //  UserQuickListItemDto can NOT have Id set OR it MUST be set to 0 
            var item = _orderService.AddQuickListItem(quickListItemBody, siteId.Value, userId.Value);

            if (item == null)
            {
                return NotFound($"New quicklist item was not added for user with id '{userId}' on site with id '{siteId}'.");
            }

            Response.Headers.Add("EMAR-Site", siteId.ToString());
            Response.Headers.Add("EMAR-User", userId.ToString());
            return CreatedAtRoute(nameof(GetQuickListItem), new { quickListItemId = item.Id }, item);
        }

        /// <summary>
        /// Create an order in the user/patient's cart as a copy of the quicklist order
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="userId">The user who owns the Cart</param>
        /// <param name="quickListItemId">The QuickList Item to move into the cart</param>
        /// <param name="patientId">the patient that the cart is intended for</param>
        /// <param name="duration">The duration of the future administrations.  Can be null.</param>
        /// <param name="durationUnitId">The ID of the unit fpr the future administrations (days, hours, minutes, etc...).  Can be null.</param>
        /// <returns></returns>
        [HttpPost("{quickListItemId}/cartOrders/{patientId}", Name = nameof(CopyQuickListItemToCart))]
        [ProducesResponseType(typeof(CartOrderDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<CartOrderDto> CopyQuickListItemToCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "quickListItemId")] int quickListItemId,
            [FromRoute(Name = "patientId")] long patientId,
            [FromQuery(Name = "duration")] int? duration,
            [FromQuery(Name = "durationUnitId")] int? durationUnitId
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
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
            };

            var newCartOrder = _orderService.CopyQuickListItemToCart(quickListItemId, resource, duration, durationUnitId);

            if (newCartOrder == null)
            {
                return NotFound($"New cart order from Quick List id '{quickListItemId}' for patient with id '{patientId}' from user with id '{userId}' was not added.");
            }

            Response.Headers.Add("EMAR-User", userId.ToString());

            return CreatedAtRoute(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = newCartOrder.Id }, newCartOrder);
        }

        /// <summary>
        /// Delete a quick list item.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="quickListItemId">
        /// Unique quick list item identifier.
        /// </param>
        /// <returns>Nothing</returns>
        [HttpDelete("delete/{quickListItemId}", Name = nameof(DeleteQuickListItem))]
        //[ProducesResponseType(200)] // (OK) - the resource has been deleted
        [ProducesResponseType(204)] // (NoContent) - the resource has been deleted
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        //[ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(410)] // (gone)
        public ActionResult DeleteQuickListItem(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "quickListItemId")] int quickListItemId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (_orderService.DeleteQuickListItem(quickListItemId))
            {
                return NoContent();
            }

            return BadRequest($"Quick list Item with id '{quickListItemId}' was not deleted successfully.");
        } //end DeleteQuickListItem
    }
}
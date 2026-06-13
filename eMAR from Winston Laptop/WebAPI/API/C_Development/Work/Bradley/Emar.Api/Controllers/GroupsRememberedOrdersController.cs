using System;
using Emar.Api.Helpers;
using Emar.Core.Carts.Model;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Emar.Core.ResourceParameters;
using Emar.Core.Sites.Model;
using Emar.Core.Sites.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Controller to retrieve the Groups Remembered Orders Lists for the Landing Page
    /// </summary>
    [ApiController]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class GroupsRememberedOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ISiteService _siteService;

        /// <summary>
        /// Controller to handle calls related to the Groups Remembered Orders List
        /// </summary>
        /// <param name="orderService">Order Service provided by DI</param>
        /// <param name="siteService">Site Service provided by DI</param>
        public GroupsRememberedOrdersController(IOrderService orderService, ISiteService siteService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }

        /// <summary>
        /// Return the contents Groups Remembered Orders  List
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="userId">The user to retrieve the Groups Remembered Orders list for (provided in the header)</param>
        /// <param name="siteId">The Site to retrieve the Groups Remembered Orders List for</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart.  Also, perform drug interaction checking against current patient orders, patient cart orders and patient home medications as well as allergy reaction checking.</param>
        /// <param name="departmentCode">(Optional) If provided, the list will be for a specific department (if not provided, the entire list for the Site)</param>
        /// <returns></returns>
        [HttpGet("api/sites/{siteId}/groupsRememberedOrdersLists", Name = nameof(GetGroupsRememberedOrdersList))]
        [ProducesResponseType(typeof(GroupsRememberedOrdersDto), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<GroupsRememberedOrdersDto> GetGroupsRememberedOrdersList(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-Patient")] long? patientId,
            [FromHeader(Name = "EMAR-Department")] string departmentCode,
            [FromRoute(Name = "siteId")] int siteId
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
                SiteId = siteId,
                UserId = userId.Value,
                PatientId = patientId ?? 0,
                LinkCopyItemToCart = Url.Link(nameof(CopyGroupsRememberedItemToCart), new { groupsRememberedItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.UserQuickListItem, itemId = -99 })
            };

            var ret = _orderService.GetGroupsRememberedOrdersList(departmentCode, resource);

            if (ret != null) return Ok(ret);

            var site = _siteService.GetSite(siteId);

            return NotFound(departmentCode == null
                ? $"No Groups Remembered List Items found for site '{site.Name}'"
                : $"No Groups Remembered List Items found for site '{site.Name}', department: '{departmentCode}'");
        }

        /// <summary>
        /// Create an order in the user/patient's cart as a copy of the Groups Remembered List order
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="userId">The user who is placing the order in the cart</param>
        /// <param name="groupsRememberedItemId">The Groups Remembered List Item to move into the patient's cart</param>
        /// <param name="patientId">the patient that the cart is intended for</param>
        /// <returns></returns>
        [HttpPost("api/patients/{patientId}/groupsRememberedOrdersLists/{groupsRememberedItemId}/cartOrders", Name = nameof(CopyGroupsRememberedItemToCart))]
        [ProducesResponseType(typeof(CartOrderDto), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<CartOrderDto> CopyGroupsRememberedItemToCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId,
            [FromRoute(Name = "groupsRememberedItemId")] int groupsRememberedItemId
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
                LinkCopyItemToCart = Url.Link(nameof(CopyGroupsRememberedItemToCart), new { departmentPreferredItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.UserQuickListItem, itemId = -99 })
            };

            var newCartOrder = _orderService.CopyGroupRememberedOrderItemToCart(groupsRememberedItemId, resource);

            if (newCartOrder == null)
            {
                return NotFound($"New cart order from Group Remembered List id '{groupsRememberedItemId}' for patient with id '{patientId}' from user with id '{userId}' was not added.");
            }

            Response.Headers.Add("EMAR-User", userId.ToString());

            return CreatedAtRoute(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = newCartOrder.Id }, newCartOrder);
        }
    }
}
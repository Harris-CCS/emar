using System;
using System.Collections.Generic;
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
    /// Controller to retrieve the Department Preferred Lists for the Landing Page
    /// </summary>
    [ApiController]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class DepartmentPreferredListsController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ISiteService _siteService;

        /// <summary>
        /// Controller to handle calls related to the Department Preferred List
        /// </summary>
        /// <param name="orderService">Order Service provided by DI</param>
        /// <param name="siteService">Site Service provided by DI</param>
        public DepartmentPreferredListsController(IOrderService orderService, ISiteService siteService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }

        /// <summary>
        /// Return the Department Preferred List for the current site.
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="siteId">The Site to retrieve the Department Preferred list for</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart.  Also, perform drug interaction checking against current patient orders, patient cart orders and patient home medications as well as allergy reaction checking.</param>
        /// <param name="departmentCode">(Optional) If provided, the list will be for a specific department (if not provided, the entire list for the Site)</param>
        /// <returns></returns>
        [HttpGet("api/sites/{siteId}/departmentPreferredLists", Name = nameof(GetDepartmentPreferredList))]
        [ProducesResponseType(typeof(IEnumerable<DepartmentPreferredItemDto>), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<DepartmentPreferredItemDto>> GetDepartmentPreferredList(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Patient")] long? patientId,
            [FromHeader(Name = "EMAR-Department")] string departmentCode,
            [FromHeader(Name = "EMAR-User")] int? userId, 
            [FromRoute(Name = "siteId")] int siteId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var resource = new BaseLinkResource
            {
                SiteId = siteId,
                PatientId = patientId ?? 0,
                UserId = userId ?? 0,
                LinkCopyItemToCart = Url.Link(nameof(CopyDepartmentPreferredItemToCart), new { departmentPreferredItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.DepartmentPreferredListItem, itemId = -99 })
            };

            var ret = _orderService.GetDepartmentPreferredList(departmentCode, resource);

            if (ret != null) return Ok(ret);

            var site = _siteService.GetSite(siteId);

            return NotFound(departmentCode == null
                ? $"No Department Preferred Items found for site '{site.Name}'"
                : $"No Department Preferred Items found for site '{site.Name}', department: '{departmentCode}'");
        }

        /// <summary>
        /// Return the Department Preferred List for the current site by the tab name.
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="siteId">The Site to retrieve the Department Preferred list for</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart.  Also, perform drug interaction checking against current patient orders, patient cart orders and patient home medications as well as allergy reaction checking.</param>
        /// <param name="departmentCode">(Optional) If provided, the list will be for a specific department (if not provided, the entire list for the Site)</param>
        /// <param name="tabName">Either "Initial" or a letter of the alphabet.</param>
        /// <returns></returns>
        [HttpGet("api/sites/{siteId}/departmentPreferredLists/tabs/{tabName}", Name = nameof(GetDepartmentPreferredListByTab))]
        [ProducesResponseType(typeof(IEnumerable<DepartmentPreferredItemDto>), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<DepartmentPreferredItemDto>> GetDepartmentPreferredListByTab(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Patient")] long? patientId,
            [FromHeader(Name = "EMAR-Department")] string departmentCode,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "siteId")] int siteId,
            [FromRoute(Name = "tabName")] string tabName

            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var resource = new BaseLinkResource
            {
                SiteId = siteId,
                PatientId = patientId ?? 0,
                UserId = userId ?? 0,
                LinkCopyItemToCart = Url.Link(nameof(CopyDepartmentPreferredItemToCart), new { departmentPreferredItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.DepartmentPreferredListItem, itemId = -99 })
            };

            //Make a different service/repository call, based on if tabName is Initial or not.
            if (tabName.ToLower() == "initial")
            {
                //Initial.

                //Get the absolute link to api/sites/{siteId}/departmentPreferredLists/tabs/.
                //We'll append the tab letter onto the end of it later on.
                var tabLinkBase = Url.Link(nameof(GetDepartmentPreferredListByTab), new { tabTitle = "C" });
                tabLinkBase = tabLinkBase.Substring(0, tabLinkBase.LastIndexOf("/tabs/", StringComparison.InvariantCultureIgnoreCase) + 6);

                resource.LinkGetDepartmentPreferredListTab = tabLinkBase;
                
                //Return the counts for teach letter and the list for the first tab.
                var ret = _orderService.GetInitialDepartmentPreferredList(departmentCode, resource);

                if (ret != null) return Ok(ret);

                var site = _siteService.GetSite(siteId);

                return NotFound(departmentCode == null
                    ? $"No Department Preferred Items found for site '{site.Name}'"
                    : $"No Department Preferred Items found for site '{site.Name}', department: '{departmentCode}'");
            }
            else
            {
                //Retuen the list for the parameter tab.
                var ret = _orderService.GetDepartmentPreferredListByTab(departmentCode, resource, tabName);

                if (ret != null) return Ok(ret);

                var site = _siteService.GetSite(siteId);

                return NotFound(departmentCode == null
                    ? $"No Department Preferred Items found for site '{site.Name}, tab: {tabName}'"
                    : $"No Department Preferred Items found for site '{site.Name}', department: '{departmentCode}, tab: {tabName}'");
            } //end if
        }

        /// <summary>
        /// Create an order in the user/patient's cart as a copy of the Department Preferred List order
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header</param>
        /// <param name="userId">The user who is placing the order in the cart</param>
        /// <param name="departmentPreferredItemId">The Department Preferred List Item to move into the patient's cart</param>
        /// <param name="patientId">the patient that the cart is intended for</param>
        /// <returns></returns>
        [HttpPost("api/patients/{patientId}/departmentPreferredLists/{departmentPreferredItemId}/cartOrders", Name = nameof(CopyDepartmentPreferredItemToCart))]
        [ProducesResponseType(typeof(CartOrderDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<CartOrderDto> CopyDepartmentPreferredItemToCart(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId,
            [FromRoute(Name = "departmentPreferredItemId")] int departmentPreferredItemId
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
                LinkCopyItemToCart = Url.Link(nameof(CopyDepartmentPreferredItemToCart), new { departmentPreferredItemId = -99, patientId = patientId }),
                LinkGetPatientOrder = Url.Link(nameof(OrdersController.GetOrder), new { orderId = -99 }),
                LinkGetCartOrder = Url.Link(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = -99 }),
                LinkGetHomeMedication = Url.Link(nameof(HomeMedicationsController.GetHomeMedication), new { homeMedicationId = -99 }),
                LinkGetSchedulerOptionsListItem = Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.UserQuickListItem, itemId = -99 })
            };

            var newCartOrder = _orderService.CopyDepartmentPreferredItemToCart(departmentPreferredItemId, resource);

            if (newCartOrder == null)
            {
                return NotFound($"New cart order from Department Preferred List id '{departmentPreferredItemId}' for patient with id '{patientId}' from user with id '{userId}' was not added.");
            }

            Response.Headers.Add("EMAR-User", userId.ToString());

            return CreatedAtRoute(nameof(CartOrdersController.GetCartOrder), new { cartOrderId = newCartOrder.Id }, newCartOrder);
        }
    }
}
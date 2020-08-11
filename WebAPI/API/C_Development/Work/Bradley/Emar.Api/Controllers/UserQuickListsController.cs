using System;
using System.Collections.Generic;
using Emar.Api.Helpers;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
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
        /// <param name="userId">The user to retrieve the quick list for (provided in the header)</param>
        /// <param name="siteId">(Optional) The Site to retrieve the user's quick list for (if omitted, return the user's quick list for all sites)</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart</param>
        /// <returns></returns>
        [HttpGet(Name = nameof(GetUserQuickListInitial))]
        [ProducesResponseType(typeof(UserQuickListFrameworkDto), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserQuickListFrameworkDto> GetUserQuickListInitial(
            [FromHeader(Name = "X-User")] int userId,
            [FromQuery] int? siteId,
            [FromQuery] long? patientId)
        {
            var tabLinkBase = Url.Link(nameof(GetUserQuickListTab), new { tabTitle = "C" });
            tabLinkBase = tabLinkBase.Substring(0, tabLinkBase.LastIndexOf("/tabs/", StringComparison.InvariantCultureIgnoreCase) + 6);

            string orderLinkBase = null;
            if ((patientId ?? -1) > 0)
                orderLinkBase = Url.Link(nameof(CopyQuickListItemToCart),
                    new { quickListItemId = -99, patientId = patientId });

            //            var link = CreateOrdersResourceUri(resourceParameters: resourceParameters, ResourceUriType.TabPage);
            UserQuickListFrameworkDto ret = _orderService.GetInitialUserQuickList(userId, siteId, tabLinkBase, orderLinkBase);

            if (ret == null)
                return NotFound($"User with id {userId} does not exist");

            //var links = CreateHateOasLinksForQuickListFramework(ret);

            return Ok(ret);
        }

        /// <summary>
        /// Return the contents for one tab of a User's Quick List
        /// </summary>
        /// <param name="userId">The user to retrieve the quick list for (provided in the header)</param>
        /// <param name="siteId">(Optional) The Site to retrieve the user's quick list for (if omitted, return the user's quick list for all sites)</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart</param>
        /// <param name="tabTitle">the tab to retrieve remembered orders for</param>
        /// <returns></returns>
        [HttpGet("tabs/{tabTitle}", Name = "GetUserQuickListTab")]
        [ProducesResponseType(typeof(IEnumerable<UserQuickListItemDto>), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<UserQuickListItemDto>> GetUserQuickListTab(
            [FromHeader(Name = "X-User")] int userId,
            [FromQuery] int? siteId,
            [FromQuery] long? patientId,
            [FromRoute] string tabTitle)
        {
            var orderLinkBase = Url.Link(nameof(CopyQuickListItemToCart), new { quickListItemId = -99, patientId = patientId });

            IEnumerable<UserQuickListItemDto> ret = _orderService.GetQuickListTab(userId, siteId, orderLinkBase, tabTitle);

            if (ret != null) return Ok(ret);
            if (siteId == null)
                return NotFound(
                    $"User with id {userId} does not have any Quick List Orders for the '{tabTitle}' tab");
            return NotFound(
                $"User with id {userId} does not have any Quick List Orders for the '{tabTitle}' tab for Site {siteId}");
        }

        /// <summary>
        /// Create an order in the user/patient's cart as a copy of the quicklist order
        /// </summary>
        /// <param name="userId">The user who owns the Cart</param>
        /// <param name="quickListItemId">The QuickList Item to move into the cart</param>
        /// <param name="patientId">the patient that the cart is intended for</param>
        /// <returns></returns>
        [HttpPost("{quickListItemId}/cartOrders/{patientId}", Name = "CopyQuickListItemToCart")]
        [ProducesResponseType(typeof(IEnumerable<UserQuickListItemDto>), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<UserQuickListItemDto>> CopyQuickListItemToCart(
            [FromHeader(Name = "X-User")] int userId,
            int quickListItemId,
            long patientId)
        {

            return NotFound(
                $"This endpoint hasn't been coded yet.");
        }
    }
}
using System;
using System.Collections.Generic;
using Emar.Api.Helpers;
using Emar.Core.Carts.Model;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
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
        /// <param name="siteId">The Site to retrieve the Groups Remembered Orders List for</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart</param>
        /// <param name="departmentCode">(Optional) If provided, the list will be for a specific department (if not provided, the entire list for the Site)</param>
        /// <returns></returns>
        [HttpGet("api/sites/{siteId}/groupsRememberedOrdersLists", Name = "GetGroupsRememberedOrdersList")]
        [ProducesResponseType(typeof(GroupsRememberedOrdersDto), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<GroupsRememberedOrdersDto> GetGroupsRememberedOrdersList(
            [FromRoute] int siteId,
            [FromQuery] string departmentCode,
            [FromQuery] int? patientId)
        {
            var ptId = patientId;
            var linkBase = Url.Link(nameof(CopyGroupsRememberedItemToCart),
                new { patientId = ptId, groupsRememberedItemId = -99});

            GroupsRememberedOrdersDto ret =
                _orderService.GetGroupsRememberedOrdersList(siteId, departmentCode, linkBase);

            if (ret != null) return Ok(ret);

            SiteDto site = _siteService.GetSite(siteId);

            if (departmentCode == null)
                return NotFound(
                    $"No Groups Remembered List Items found for site '{site.Name}'");
            return NotFound(
                $"No Groups Remembered List Items found for site '{site.Name}', department: '{departmentCode}'");
        }

        /// <summary>
        /// Create an order in the user/patient's cart as a copy of the Groups Remembered List order
        /// </summary>
        /// <param name="userId">The user who is placing the order in the cart</param>
        /// <param name="groupsRememberedItemId">The Groups Remembered List Item to move into the patient's cart</param>
        /// <param name="patientId">the patient that the cart is intended for</param>
        /// <returns></returns>
        [HttpPost("api/patients/{patientId}/groupsRememberedOrdersLists/{groupsRememberedItemId}/cartOrders",
            Name = nameof(CopyGroupsRememberedItemToCart))]
        [ProducesResponseType(typeof(CartOrderDto), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<CartOrderDto> CopyGroupsRememberedItemToCart(
            [FromHeader(Name = "X-User")] int userId,
            int groupsRememberedItemId,
            long patientId)
        {
            //var medicationInteractionsReactions = _orderService.CopyGroupRememberedOrderItemToCart(userId, groupsRememberedItemId, patientId);

            //return Ok(medicationInteractionsReactions);
            return NotFound(
                $"This endpoint hasn't been coded yet.");
        }
    }
}
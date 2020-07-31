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
        /// <returns></returns>
        [HttpGet(Name = nameof(GetUserQuickListInitial))]
        [ProducesResponseType(typeof(UserQuickListFrameworkDto), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserQuickListFrameworkDto> GetUserQuickListInitial(
            [FromHeader(Name = "X-User")] int userId,
            [FromQuery] int? siteId)
        {
            UserQuickListFrameworkDto ret = _orderService.GetInitialUserQuickList(userId, siteId);

            if (ret == null)
                return NotFound($"User with id {userId} does not exist");

            return Ok(ret);
        }

        /// <summary>
        /// Return the contents for one tab of a User's Quick List
        /// </summary>
        /// <param name="userId">The user to retrieve the quick list for (provided in the header)</param>
        /// <param name="siteId">(Optional) The Site to retrieve the user's quick list for (if omitted, return the user's quick list for all sites)</param>
        /// <param name="tabTitle">the tab to retrieve remembered orders for</param>
        /// <returns></returns>
        [HttpGet("tabs/{tabTitle}", Name = nameof(GetUserQuickListTab))]
        [ProducesResponseType(typeof(IEnumerable<UserQuickListItemDto>), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<UserQuickListItemDto>> GetUserQuickListTab(
            [FromHeader(Name = "X-User")] int userId,
            [FromQuery] int? siteId,
            [FromRoute] string tabTitle)
        {
            IEnumerable<UserQuickListItemDto> ret = _orderService.GetQuickListTab(userId, siteId, tabTitle);

            if (ret != null) return Ok(ret);
            if (siteId == null)
                return NotFound(
                    $"User with id {userId} does not have any Quick List Orders for the '{tabTitle}' tab");
            return NotFound(
                $"User with id {userId} does not have any Quick List Orders for the '{tabTitle}' tab for Site {siteId}");
        }
    }
}
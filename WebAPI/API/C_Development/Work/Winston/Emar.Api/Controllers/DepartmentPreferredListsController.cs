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
        /// Return the contents Department Preferred List
        /// </summary>
        /// <param name="siteId">The Site to retrieve the Department Preferred list for</param>
        /// <param name="patientId">(Optional) If provided, then HATEOAS links will be created to allow for the adding of the order directly to the patient's/user's cart</param>
        /// <param name="departmentCode">(Optional) If provided, the list will be for a specific department (if not provided, the entire list for the Site)</param>
        /// <returns></returns>
        [HttpGet("api/sites/{siteId}/departmentPreferredLists", Name = "GetDepartmentPreferredList")]
        [ProducesResponseType(typeof(IEnumerable<DepartmentPreferredItemDto>), 200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<DepartmentPreferredItemDto>> GetDepartmentPreferredList(
            [FromRoute] int siteId,
            [FromQuery] string departmentCode,
            [FromQuery] int? patientId)
        {
            var ptId = patientId;
            var linkBase = Url.Link(nameof(CopyDepartmentPreferredItemToCart),
                new { patientId = ptId, departmentPreferredItemId = -99});

            IEnumerable<DepartmentPreferredItemDto> ret =
                _orderService.GetDepartmentPreferredList(siteId, departmentCode, linkBase);

            if (ret != null) return Ok(ret);

            SiteDto site = _siteService.GetSite(siteId);

            if (departmentCode == null)
                return NotFound(
                    $"No Department Preferred Items found for site '{site.Name}'");
            return NotFound(
                $"No Department Preferred Items found for site '{site.Name}', department: '{departmentCode}'");
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
            [FromHeader(Name = "X-User")] int userId,
            int departmentPreferredItemId,
            long patientId)
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            //var newCartOrder = _orderService.CopyDepartmentPreferredItemToCart(userId, departmentPreferredItemId, patientId);

            //return Ok(newCartOrder);
            return NotFound(
                $"This endpoint hasn't been coded yet.");
        }
    }
}
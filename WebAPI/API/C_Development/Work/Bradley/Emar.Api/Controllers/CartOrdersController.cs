using System;
using Emar.Api.Helpers;
using Emar.Core;
using Emar.Core.Carts.Service;
using Emar.Core.Helpers;
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
    }
}
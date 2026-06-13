using Emar.Api.Helpers;
using Emar.Core.Devices.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Controller for devices.
    /// </summary>
    /// 
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        /// <summary>
        /// Constructor
        /// </summary>
        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        } //end constructor
        
        /// <summary>
        /// Return the active devices for a given site id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="siteId">
        /// The id of the site that we are searching in.
        /// Is pulled from the request header.
        /// </param>
        /// <param name="userId">
        /// The id of the current user.
        /// Is pulled from the request header.
        /// </param>
        /// 
        /// <returns>
        /// The list of active devices for a given site id.
        /// If the user has a last used device, that one is listed first.
        /// Then the remaining active devices are listed sorted by description.
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/devices/devices", Name = nameof(GetDevicesBySite))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetDevicesBySite
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId
        )
        {
            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }
            
            //Check the site id.
            if (siteId == null)
            {
                return BadRequest("Site ID is missing.");
            }

            //Check the user id.
            if (userId == null)
            {
                return BadRequest("The User ID is missing.");
            }

            //Get the list of devices.
            var devices = _deviceService.GetDevices((int)siteId, (int)userId);

            //Confirm that we actually have devices.
            if (devices == null)
            {
                return NotFound($"No devices found for site ID '{siteId}'.");
            }

            return Ok(devices);
        }
    }
}

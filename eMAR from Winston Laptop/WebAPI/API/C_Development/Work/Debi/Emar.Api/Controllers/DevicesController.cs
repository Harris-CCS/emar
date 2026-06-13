using Emar.Api.Helpers;
using Emar.Core.Devices.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

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
        [HttpGet("api/devices/devices/site/{siteId}", Name = nameof(GetDevicesBySite))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetDevicesBySite
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "siteId")] int? siteId,
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
            if (!devices.Any())
            {
                return NotFound($"No devices found for site ID '{siteId}'.");
            }
            
            return Ok(devices);
        } //end GetDevicesBySite

        /// <summary>
        /// Accept the binary data of a file we need to print, save the info to the print_history
        /// table, and then write the file to ibex\temp on the DB server so that Jim's stuff in
        /// PulseCheck can print the document.
        /// Inbound JSON structure is...
        /// {
        ///    "user_id_printing": 8404,
        ///    "device_id": 17,
        ///    "patient_id": 699,
        ///    "description": "MAR Patient Report",
        ///    "document_type": "file",
        ///    "file_name": "eMarU8404y2021m5d3h12m2s53c55.pdf",
        ///    "file_format": "pdf",
        ///    "page_count": 3,
        ///    "date_time": "2021-02-21 18:05:00.6264503 -06:00",
        ///    "content": "base64 string here",
        ///    "expiration_documentation": "2021-02-21 18:05:00.6264503 -06:00"
        ///}

    /// </summary>
    /// <param name="mediaType">
    /// Media type from Accept header.
    /// </param>
    /// <param name="printFileResponses">
    /// The request body.  Donald and Winston will iron out the field names.
    /// </param>
    /// <remarks>
    /// </remarks>
    [HttpPost("api/devices/print/", Name = nameof(SavePrintFile))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> SavePrintFile
        (
            [FromHeader(Name = "Accept")] string mediaType, 
            //Will need to get with Donald to get the names of these fields.
            [FromBody] Dictionary<string, string> printFileResponses
        )
        {
            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var sPdfPath = _deviceService.SavePrintFile(printFileResponses);
            var printReturn = _deviceService.MakePrintFile(printFileResponses, sPdfPath);
            //Return the path to the PDF file.
            //The UI doesn't need it, but that way we return something.
            return Ok(sPdfPath);
        } //end SavePrintFile
    }
}

using Emar.Api.Helpers;
using Emar.Core.WinstonTests.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Controller for WinstonTests.
    /// </summary>
    /// 
    public class WinstonTestsController : ControllerBase
    {
        private readonly IWinstonTestService _winstonTestService;

        /// <summary>
        /// Constructor
        /// </summary>
        public WinstonTestsController(IWinstonTestService deviceService)
        {
            _winstonTestService = deviceService;
        } //end constructor

        /// <summary>
        /// Return all Winston Tests.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// 
        /// <returns>
        /// The list of Winston Tests.
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/winstontests/getwinstontests", Name = nameof(GetWinstonTests))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetWinstonTests
        (
            [FromHeader(Name = "Accept")] string mediaType
        )
        {
            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            //Get the list of devices.
            var winstonTests = _winstonTestService.GetWinstonTests();

            //Confirm that we actually have devices.
            if (!winstonTests.Any())
            {
                return NotFound($"No Winston Tests found.");
            }

            return Ok(winstonTests);
        } //end GetDevicesBySite

        /// <summary>
        /// Return all active Winston Tests.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// 
        /// <returns>
        /// The list of Winston Tests.
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/winstontests/getactivewinstontests", Name = nameof(GetActiveWinstonTests))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetActiveWinstonTests
        (
            [FromHeader(Name = "Accept")] string mediaType
        )
        {
            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            //Get the list of devices.
            var winstonTests = _winstonTestService.GetActiveWinstonTests();

            //Confirm that we actually have devices.
            if (!winstonTests.Any())
            {
                return NotFound($"No Winston Tests found.");
            }

            return Ok(winstonTests);
        } //end GetDevicesBySite

        /// <summary>
        /// Return all Winston Test sorted by name.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// 
        /// <returns>
        /// The list of Winston Tests sorted by name.
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/winstontests/getwinstontestssortbycolumnoneascending", Name = nameof(GetAllWinstonTestsAscending))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetAllWinstonTestsAscending
        (
            [FromHeader(Name = "Accept")] string mediaType
        )
        {
            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            //Get the list of devices.
            var winstonTests = _winstonTestService.GetWinstonTestsSortByColumnOneAscending();

            //Confirm that we actually have devices.
            if (!winstonTests.Any())
            {
                return NotFound($"No Winston Tests found.");
            }

            return Ok(winstonTests);
        } //end GetDevicesBySite

        /// <summary>
        /// Return all Winston Tests sorted in reverse order.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// 
        /// <returns>
        /// The list of Winston Tests sorted in reverse order.
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/winstontests/getwinstontestssortbycolumnonedescending", Name = nameof(GetAllWinstonTestsDescending))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetAllWinstonTestsDescending
        (
            [FromHeader(Name = "Accept")] string mediaType
        )
        {
            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            //Get the list of devices.
            var winstonTests = _winstonTestService.GetWinstonTestsSortByColumnOneDescending();

            //Confirm that we actually have devices.
            if (!winstonTests.Any())
            {
                return NotFound($"No Winston Tests found.");
            }

            return Ok(winstonTests);
        } //end GetDevicesBySite
    }
}

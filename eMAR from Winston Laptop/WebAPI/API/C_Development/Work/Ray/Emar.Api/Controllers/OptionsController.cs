using Emar.Api.Helpers;
using Emar.Core.Options.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Controller for site options.
    /// </summary>
    /// 
    [ApiController]
    public class OptionsController : ControllerBase
    {
        private readonly IOptionService _optionService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsController(IOptionService optionService)
        {
            _optionService = optionService;
        } //end constructor

        /// <summary>
        /// Return the global options and the specified site options for a given site id.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="siteId">
        /// The id of the site that we are searching in.
        /// Is pulled from the request header.
        /// </param>
        /// <param name="options">
        /// A comma-delimited list of the options that are desired.
        /// These must be exactly one of the values in options.name.
        /// If all site options and all global options are desired then
        /// pass in "all" as the value for this parameter.
        /// If anything other than "all" is passed in, then we'll return the
        /// specified site options and all global options.
        /// </param>
        /// 
        /// <returns>
        /// The specified site-specific options and the global options.
        /// The site-specific options are alphabatized and come first.
        /// And then the global options are alphabatized and come second.
        /// </returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/siteoptions/{options}", Name = nameof(GetSiteOptionsBySite))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<int> GetSiteOptionsBySite
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromRoute(Name = "options")] string options
        )
        {
            //return variable.
            var optionList = new Dictionary<string, string>();

            //Check the media type.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            } //end if (media type)

            //Check the site ID.
            //Winston Murdock, 11/20/2020.  EMAR-508.
            if (siteId == null)
            {
                return BadRequest("Site ID is missing.");
            } //end if (site ID)

            //Get the global options and the site-specific options.
            //If the value of "options" is "all", then they want all site options and all global options.
            //If the value is anything else, then we'll get the global options
            //and the site options that they specified.
            optionList = _optionService.GetSiteOptions(siteId.Value, options);
            
            //If no options were returned, then let the UI know.
            if (optionList == null || optionList.Count < 1)
            {
                return BadRequest("No global or site options found.");
            } //end if (no options found)

            //Return the list.
            return Ok(optionList);
        }
    }
}

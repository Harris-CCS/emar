using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Api.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Dose Range Checking Info Controller
    /// </summary>
    public class MedicationsController : Controller
    {
        private readonly IDoseRangeCheckingInfoService _doseRangeCheckingInfoService;

        /// <summary>
        /// Constructor
        /// </summary>
        public MedicationsController(IDoseRangeCheckingInfoService service)
        {
            _doseRangeCheckingInfoService = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Get the dose range checking info for a medication.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="ndc">
        /// Unique order identifier.
        /// </param>
        /// <returns>The dose range info for the medication that has the passed in NDC</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/GetDoseRangeCheckingInfo/{ndc}", Name = nameof(GetDoseRangeCheckingInfo))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<DoseRangeCheckingInfoDto>> GetDoseRangeCheckingInfo
        (
            [FromHeader(Name = "Accept")] string mediaType,
            string ndc
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (ndc == null)
            {
                return BadRequest("NDC is missing.");
            }

            var doseRangeCheckInfos = _doseRangeCheckingInfoService.DoseRangeCheckInfos(ndc);

            if (doseRangeCheckInfos == null || !doseRangeCheckInfos.Any())
            {
                return NotFound($"Dose Range Checking Info for ndc {ndc} was not found.");
            }

            return Ok(doseRangeCheckInfos);
        } //end GetDoseRangeCheckingInfo

        /// <summary>
        /// Medication search by brand name
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="brandName">
        /// The medication brand name we are searching for.
        /// </param>
        /// <param name="siteId">
        /// The id of the site that we are searching in.
        /// </param>
        /// <param name="searchType">
        /// The type of list to search in.
        /// quicklist = user's quick list
        /// deptpreferredlist =  department preferred list
        /// groups =  groups
        /// formulary =  formulary
        /// empty = all
        /// </param>
        /// <param name="userId">
        /// The id of the current user.
        /// Pulled from the request header.
        /// Used when searching in the user-specific quick list.
        /// </param>
        /// 
        /// <returns>TBD</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/GetMedByBrandName/{brand_name}/{site_id}/{search_type}", Name = nameof(GetDoseRangeCheckingInfo))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<DoseRangeCheckingInfoDto>> GetMedByBrandName
        (
            [FromHeader(Name = "Accept")] string mediaType,
            string brandName,
            int siteId,
            string? searchType,
            [FromHeader(Name = "X-User")] int userId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (brandName == null)
            {
                return BadRequest("Brand Name is missing.");
            }


            //Figure out which drug knowledge vendor we're using so that we know which repository and service to use.
            //Since it is possible for a client to have FDB at one site and Multum at another site, we cannot
            //set this at startup.
            //I envision startup having a repository and service setup at startup for each of the vendors.
            //Then we'll pick the one we need to use here.
            //I'll need to look in the options

            //Need to check what type of search we're doing (based on the searchType param).
            if (searchType == "quicklist")
            {
                //Searching in the user-specific quick list.
                return Ok(searchType);
            }
            else if (searchType == "deptpreferredlist")
            {
                //Searching in the department's prefered list.
                return Ok(searchType);
            }
            else if (searchType == "groups")
            {
                //Searching in groups.
                return Ok(searchType);
            }
            else if (searchType == "formulary")
            {
                //Searching in the site-specific formulary.
                return Ok(searchType);
            }
            else
            {
                //Searching in all of FDB.
                return Ok("All");
            }

            //var doseRangeCheckInfos = _doseRangeCheckingInfoService.DoseRangeCheckInfos(ndc);

            //If no medications in return list...
            //if (doseRangeCheckInfos == null || !doseRangeCheckInfos.Any())
            //{
            //    return NotFound($"Dose Range Checking Info for ndc {ndc} was not found.");
            //}

            //return Ok(doseRangeCheckInfos);
        }
    }
}

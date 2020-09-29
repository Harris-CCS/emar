using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Api.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Service;
using Emar.Core.Options.Service;
using Microsoft.AspNetCore.Mvc;
namespace Emar.Api.Controllers
{
    /// <summary>
    /// Dose Range Checking Info Controller
    /// </summary>
    public class MedicationsController : Controller
    {
        private readonly IDoseRangeCheckingInfoService _doseRangeCheckingInfoService;
        private readonly IMedicationService _medicationService;
        private readonly IOptionService _optionService;

        /// <summary>
        /// Constructor
        /// </summary>
        public MedicationsController(IDoseRangeCheckingInfoService service, IMedicationService medService, IOptionService optService)
        {
            _doseRangeCheckingInfoService = service ?? throw new ArgumentNullException(nameof(service));
            _medicationService = medService ?? throw new ArgumentNullException(nameof(medService));
            _optionService = optService ?? throw new ArgumentNullException(nameof(optService));
        }

        /// <summary>
        /// Get the dose range checking info for a medication.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="medid">
        /// The medication/drug ID.
        /// </param>
        /// <returns>The dose range info for the medication that has the passed in NDC</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/GetDoseRangeCheckingInfo/{medid}", Name = nameof(GetDoseRangeCheckingInfo))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<DoseRangeCheckingInfoDto>> GetDoseRangeCheckingInfo
        (
            [FromHeader(Name = "Accept")] string mediaType,
            string medid
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (medid == null)
            {
                return BadRequest("medid is missing.");
            }

            if (medid.Length > 9)
            {
                return BadRequest("medid cannot be greater than 9 characters.");
            }

            var doseRangeCheckInfos = _doseRangeCheckingInfoService.DoseRangeCheckInfos(medid);

            if (doseRangeCheckInfos == null || !doseRangeCheckInfos.Any())
            {
                return NotFound($"Dose Range Checking Info for medid {medid} was not found.");
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
        /// Is pulled from the request header.
        /// </param>
        /// <param name="searchType">
        /// The type of list to search in.
        /// all = all
        /// deptpreferredlist = department preferred list
        /// formulary = formulary
        /// groups = groups
        /// quicklist = user's quick list
        /// </param>
        /// <param name="userId">
        /// The id of the current user.
        /// Is pulled from the request header.
        /// Used when searching in the user-specific quick list.
        /// </param>
        /// /// <param name="deptCode">
        /// The department of the patient.
        /// Is pulled from the request header.
        /// Used when searching in the department preferred list.
        /// </param>
        /// 
        /// <returns>A alphabetically sorted list of medication names that match the search criteria.</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/BrandNameList/{brandName}/{searchType}", Name = nameof(GetMedByBrandName))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<DoseRangeCheckingInfoDto>> GetMedByBrandName
        (
            [FromHeader(Name = "Accept")] string mediaType,
            string brandName,
            [FromHeader(Name = "X-Site")] int siteId,
            string searchType,
            [FromHeader(Name = "X-User")] int userId,
            [FromHeader(Name = "X-PatDept")] string deptCode)
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (brandName == null)
            {
                return BadRequest("Brand Name is missing.");
            }

            if (!Enum.TryParse(typeof(MedicationLookupDto.SearchType), searchType.ToLower(), out object oSearchType))
            {
                return BadRequest($"Search Type \"{searchType}\" is invalid.");
            } //end if

            MedicationLookupDto.SearchType searchTypeEnum = (MedicationLookupDto.SearchType)oSearchType;

            var medications = _medicationService.GetMedsByBrandName(siteId, brandName, userId, searchTypeEnum, deptCode);

            if (medications == null || !medications.Any())
            {
                return NotFound($"Search string \"{brandName}\" returned no medications.");
            }

            return Ok(medications);
        }
    }
}

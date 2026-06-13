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
        private readonly IMedicationService _medicationService;

        /// <summary>
        /// Constructor
        /// </summary>
        public MedicationsController(IDoseRangeCheckingInfoService service, IMedicationService medService)
        {
            _doseRangeCheckingInfoService = service ?? throw new ArgumentNullException(nameof(service));
            _medicationService = medService ?? throw new ArgumentNullException(nameof(medService));
        }

        /// <summary>
        /// Get the dose range checking info for a medication.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="medicationId">
        /// The medication/drug ID.
        /// </param>
        /// <returns>The dose range info for the medication that has the passed in NDC</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/GetDoseRangeCheckingInfo/{medicationId}", Name = nameof(GetDoseRangeCheckingInfo))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<DoseRangeCheckingInfoDto>> GetDoseRangeCheckingInfo
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "medicationId")] int medicationId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var doseRangeCheckInfos = _doseRangeCheckingInfoService.DoseRangeCheckInfos(medicationId);

            if (doseRangeCheckInfos == null || !doseRangeCheckInfos.Any())
            {
                return NotFound($"Dose Range Checking Info for medicationId '{medicationId}' was not found.");
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
        /// The type of list item to search for
        /// (All / UserQuickListItem / DepartmentPreferredListItem / GroupRememberedOrder / FormularyItem).</param>
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
        /// <returns>A alphabetically sorted list of medication names that match the search criteria.</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/BrandNameList/{brandName}/{searchType}", Name = nameof(GetMedByBrandName))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<string>> GetMedByBrandName
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-PatientDepartment")] string deptCode,
            [FromRoute(Name = "brandName")] string brandName,
            [FromRoute(Name = "searchType")] string searchType
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site ID is missing.");
            }

            if (!Enum.TryParse(searchType.ToLower(), true, out EmarOrderType searchTypeEnum))
            {
                return BadRequest($"Search type '{searchType}' is invalid.");
            }

            if (searchTypeEnum == EmarOrderType.UserQuickListItem && userId == null)
            {
                return BadRequest($"Search type is '{searchTypeEnum}' and the user id is missing.");
            }

            if (searchTypeEnum == EmarOrderType.DepartmentPreferredListItem && deptCode == null)
            {
                return BadRequest($"Search type is '{searchTypeEnum}' and the department code is missing.");
            }

            string schedulerDataRetrieveBase =
                Url.Link(nameof(SchedulerController.GetSchedulerOptions), new {brandName = "-99"})
                    .Replace("-99", "{0}");

            var medications =
                 _medicationService.GetMedsByBrandName(siteId.Value, brandName, userId ?? 0, searchTypeEnum, deptCode,
                     schedulerDataRetrieveBase);

            if (medications == null || !medications.Any())
            {
                return NotFound($"Search string '{brandName}' returned no medications.");
            }

            return Ok(medications);
        } //end GetMedByBrandName

        /// <summary>
        /// Medication search by brand name
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="siteId">
        /// The id of the site that we are searching in.
        /// Is pulled from the request header.
        /// </param>
        /// <returns>A alphabetically sorted list of antimicrobial indications for the current site.</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet("api/Indications", Name = nameof(GetIndicationsBySite))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<string>> GetIndicationsBySite
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site ID is missing.");
            }

            //Service call here.
            var indications = _medicationService.GetIndicationsBySite(siteId.Value);

            //We might not need this.  If the site doesn't have any indications,
            //then we could probably just return an empty list.
            if (indications == null || !indications.Any())
            {
                return NotFound($"No indications found for this site.");
            }

            return Ok(indications);
        } //end GetIndicationsBySite

        /// <summary>
        /// Get whether or not each search type should be made available in the UI.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="siteId">
        /// The id of the site that we are searching in.
        /// Is pulled from the request header.
        /// </param>
        /// 
        /// <returns>True/False for each type of search.</returns>
        /// <remarks>
        /// This defaults department preferred, groups, and user quick list to true.
        /// Per product, those will always be available.
        /// The others (formulary and all are based on the data).
        /// If any of the formulary filters are turned on, then this formulary is true while all is false.
        /// If all of the formulary filters are turned off, and there is data in the formulary table, then both formulary and all will be true.
        /// If all of the forumlary filters are turned off, but there is no data in the formulary table, then formulary is false and all is true.
        /// </remarks>
        [HttpGet("api/SearchDropdownList", Name = nameof(GetSearchDropdownList))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<string>> GetSearchDropdownList
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site ID is missing.");
            }

            var searchDropdownList = _medicationService.GetSearchDropdownList(siteId.Value);

            return Ok(searchDropdownList);
        } //end GetSearchDropdownList

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="medicationId"></param>
        /// <returns></returns>
        [HttpGet("api/medications/{medicationId}", Name = nameof(GetMedication))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<MedicationDto> GetMedication(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "medicationId")] int medicationId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var details = _medicationService.GetMedication(medicationId);

            if (details == null)
            {
                return NotFound($"No medication found with id '{medicationId}'.");
            }

            return Ok(details);
        }

        /// <summary>
        /// Get a list of possible drug interactions and allergy reactions an order may have.
        /// </summary>
        /// <param name="mediaType">Media type from the "Accept" header.</param>
        /// <param name="userId">Unique user identifier.</param>
        /// <param name="patientId">Unique patient identifier.</param>
        /// <param name="itemType">Order type (UserQuickListItem / DepartmentPreferredListItem / GroupRememberedOrder / MedicationItem)</param>
        /// <param name="itemId">Unique order identifier; unique medication identifier if itemType is MedicationItem.</param>
        /// <returns></returns>
        [HttpGet("/api/interactions/{itemType}/{itemId}", Name = nameof(GetInteractionsReactions))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<MedicationInteractionReaction>> GetInteractionsReactions(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-Patient")] long? patientId,
            [FromRoute(Name = "itemType")] string itemType,
            [FromRoute(Name = "itemId")] int itemId
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

            if (!Enum.TryParse(itemType.ToLower(), true, out EmarOrderType itemTypeEnum))
            {
                return BadRequest($"Item type '{itemType}' is invalid.");
            }

            var interactionsReactions = _medicationService.GetInteractionsReactions(userId.Value, patientId.Value, itemTypeEnum, itemId);

            if (interactionsReactions == null)
            {
                return NotFound($"No drug interactions or allergy reactions found for patient with id '{patientId}' on '{itemType}' with id '{itemId}'.");
            }

            return Ok(interactionsReactions);
        }
    }
}
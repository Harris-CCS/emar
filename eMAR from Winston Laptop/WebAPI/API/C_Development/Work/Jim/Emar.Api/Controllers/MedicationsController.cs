using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Api.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Medications.Service;
using Emar.Core.Orders.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Dose Range Checking Info Controller
    /// </summary>
    [ApiController]
    public class MedicationsController : Controller
    {
        private readonly IDoseRangeCheckingInfoService _doseRangeCheckingInfoService;
        private readonly IMedicationService _medicationService;
        private readonly IOrderService _orderService;

        /// <summary>
        /// Constructor
        /// </summary>
        public MedicationsController(IDoseRangeCheckingInfoService service, IMedicationService medService, IOrderService orderService)
        {
            _doseRangeCheckingInfoService = service ?? throw new ArgumentNullException(nameof(service));
            _medicationService = medService ?? throw new ArgumentNullException(nameof(medService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
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
        [HttpGet("api/BrandNameList/{brandName}/{searchType}/site/{siteId}", Name = nameof(GetMedByBrandName))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ResponseCache(CacheProfileName = "DisableCaching")]
        public ActionResult<IEnumerable<string>> GetMedByBrandName
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "siteId")] int? siteId,
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
            
            //We can do a department preferred list search without a department code.
            //If it exists, then we use it in the filter.
            //If it doesn't exist, then we don't use it in the filter.
            //if (searchTypeEnum == EmarOrderType.DepartmentPreferredListItem && deptCode == null)
            //{
            //    return BadRequest($"Search type is '{searchTypeEnum}' and the department code is missing.");
            //}

            string schedulerDataRetrieveBase =
                Url.Link(nameof(SchedulerController.GetSchedulerOptions), new {brandName = "-99", siteId = siteId })
                    .Replace("-99", "{0}");

            //Added a call to DecodeString here since the brand name could have a special character in it that the UI encoded.
            //1) / (forward slash) = %2f
            //2) + (plus sign) = % 2b
            //3) # (hash tag/pound sign) = %23
            //Likely others as well, but these are the three I found issues with when testing locally.
            //The UI should be able to encode these before sending me the search string.
            //Winston Murdock, 07/26/2022.
            var medications =
                 _medicationService.GetMedsByBrandName(siteId.Value, DecodeString(brandName), userId ?? 0, searchTypeEnum, deptCode,
                     schedulerDataRetrieveBase, null);

            if (medications == null || !medications.Any())
            {
                return NotFound($"Search string '{brandName}' returned no medications.");
            }

            return Ok(medications);
        } //end GetMedByBrandName

        /// <summary>
        /// Medication search by brand name with the brand name string being in a request header.
        /// This does require "?r=randomNumber" to be added to the URL so that the browser doesn't
        /// cache the results for the first drug the user searched for and then use that for all
        /// subsequent searches.
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
        [HttpGet("api/BrandNameList/{searchType}/site/{siteId}", Name = nameof(GetMedByBrandNameValueInHeader))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ResponseCache(CacheProfileName = "DisableCaching")]
        public ActionResult<IEnumerable<string>> GetMedByBrandNameValueInHeader
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "siteId")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromHeader(Name = "EMAR-PatientDepartment")] string deptCode,
            [FromHeader(Name = "brandName")] string brandName,
            [FromRoute(Name = "searchType")] string searchType
        )
        {
            //Make a copy of the brand name search method that takes the search string in from the
            //request header instead of the route.
            //We made a similar change for the scheduler options page, and that resolved the
            //encoding issue with special characters in a medication name.
            //And we want to do the same here.
            //The existing endpoint was not removed.
            //Whenever the UI switches over from the existing endpoint to this one, we'll be good.
            //Winston Murdock, 08/29/2022.  PC-27488
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

            //We can do a department preferred list search without a department code.
            //If it exists, then we use it in the filter.
            //If it doesn't exist, then we don't use it in the filter.
            //if (searchTypeEnum == EmarOrderType.DepartmentPreferredListItem && deptCode == null)
            //{
            //    return BadRequest($"Search type is '{searchTypeEnum}' and the department code is missing.");
            //}


            //We took the brand name out of the search string and moved it
            //to the request header when loading the scheduler options page.
            //Also, these were pointing to the old URL that needed the brand
            //name in the URL rather than the new ones that accept it in the
            //request header.            
            //Winston Murdock, 10/06/2022.
            string schedulerDataRetrieveBase = "";

            if (searchType == EmarOrderType.All.ToString())
            {
                schedulerDataRetrieveBase = Url.Link(nameof(SchedulerController.GetSchedulerOptionsAllSearchStringInheader), new {siteId = siteId })
                        .Replace("-99", "{0}");
            }
            else
            {
                schedulerDataRetrieveBase = Url.Link(nameof(SchedulerController.GetSchedulerOptionsSearchStringInheader), new {siteId = siteId })
                        .Replace("-99", "{0}");
            } //end if

            //The service method only needs the groupLink if this is a "group" search.
            string groupLink =
                Url.Link(nameof(SchedulerController.GetSchedulerOptionsListItem), new { itemType = EmarOrderType.GroupRememberedOrder, itemId = -99 })
                    .Replace("-99", "{0}");

            //Added a call to DecodeString here since the brand name could have a special character in it that the UI encoded.
            //1) / (forward slash) = %2f
            //2) + (plus sign) = % 2b
            //3) # (hash tag/pound sign) = %23
            //Likely others as well, but these are the three I found issues with when testing locally.
            //The UI should be able to encode these before sending me the search string.
            //Winston Murdock, 07/26/2022.
            var medications =
                 _medicationService.GetMedsByBrandName(siteId.Value, DecodeString(brandName), userId ?? 0, searchTypeEnum, deptCode,
                     schedulerDataRetrieveBase, groupLink);

            if (medications == null || !medications.Any())
            {
                return NotFound($"Search string '{brandName}' returned no medications.");
            }

            return Ok(medications);
        } //end GetMedByBrandNameValueInHeader

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
        [HttpGet("api/Indications/site/{siteId}", Name = nameof(GetIndicationsBySite))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        public ActionResult<IEnumerable<string>> GetIndicationsBySite
        (
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "siteId")] int? siteId
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

            //var interactionsReactions = _medicationService.GetInteractionsReactions(userId.Value, patientId.Value, itemTypeEnum, itemId);
            //IEnumerable<MedicationInteractionReaction> interactionsReactions;
            List<MedicationInteractionReaction> interactionsReactions = new List<MedicationInteractionReaction>();

            //Handle group orders differently from the rest by calilng a
            //different function than we do for all of the other types.
            //If it's a group order that is only one order, then we'll
            //do the same stuff as we do for other order types.
            //But if the group order is a combo med, then we'll check the
            //interactions and reactions for each of the medications in the
            //combo med and return them with the combo med.
            //Winston Murdock, 04/26/2022.
            if (itemTypeEnum == EmarOrderType.GroupRememberedOrder)
            {
                interactionsReactions = _medicationService.GetGroupOrderInteractionsReactions(userId.Value, patientId.Value, itemId).ToList();
            }
            //A patient cart order could be a cmobo med.
            //Write a helper method to handle cart orders that has different logic for combo med orders versus regular orders.
            //It will mostly be a copy of the method called above.  But it will have soem logic from the method below in the section that deals with cart orders.
            //Winston Murdock, 02/21/2023.  PC-27804
            else if (itemTypeEnum == EmarOrderType.PatientCartOrder)
            {
                interactionsReactions = _medicationService.GetPatientCartOrderInteractionsReactions(userId.Value, patientId.Value, itemId).ToList();
            }
            //A patient order that a user is repeatingcould be a cmobo med.
            //Write a helper method to handle cart orders that has different logic for combo med orders versus regular orders.
            //It will mostly be a copy of the method called above.  But it will have soem logic from the method below in the section that deals with cart orders.
            //Winston Murdock, 02/21/2023.  PC-27804
            else if (itemTypeEnum == EmarOrderType.PatientOrder)
            {
                interactionsReactions = _medicationService.GetPatientOrderInteractionsReactions(userId.Value, patientId.Value, itemId).ToList();
            }
            else
            {
                interactionsReactions = _medicationService.GetInteractionsReactions(userId.Value, patientId.Value, itemTypeEnum, itemId).ToList();
            } //end if

            if (interactionsReactions == null)
            {
                return NotFound($"No drug interactions or allergy reactions found for patient with id '{patientId}' on '{itemType}' with id '{itemId}'.");
            }

            return Ok(interactionsReactions);
        }

        /// <summary>
        /// Recalculate all interactions and reactions for the passed in patient.
        /// Order (checkout) the cart orders.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="patientId">
        /// Unique patient identifier.
        /// </param>
        /// <returns></returns>
        [HttpGet("api/medications/RecalculateAllInteractionsReactions/Patient/{patientId}", Name = nameof(RecalculateAllInteractionsReactions))]
        [ProducesResponseType(typeof(string), 201)] // (created) - if a new resource is created, contain an entity which describes the status of the request and refers to the new resource, and a Location header.
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        //[ProducesResponseType(412)] // (precondition failed) e.g. conflict by performing conditional update
        [ProducesResponseType(415)] // (unsupported media type) - received representation is not supported
        public ActionResult RecalculateAllInteractionsReactions(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "patientId")] long patientId
            )
        {
            //Recalculate the interactions and reactions for all orders and cart orders for this patient.
            //Winston Murdock, 06/14/2022.
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (patientId == null)
            {
                return BadRequest("Patient id is missing.");
            }
            
            //Adding an optional "delete" parameter here.
            //We'll pass that on when calling the method that actually does deleting or not.
            //That way we completely mimic the prior "checkout cart" behavior
            //without having to make a completely new set of methods in the service.
            _orderService.UpdatePatientOrderInteractionsAndReactions(patientId, null, true);

            return Ok("success");
        }
        private static string DecodeString(string encodedString)
        {
            //https://stackoverflow.com/a/3847593
            //Winston Murdock, 05/13/2021.  EMAR-1001.
            string decodedString;
            while ((decodedString = Uri.UnescapeDataString(encodedString)) != encodedString)
                encodedString = decodedString;
            return decodedString;
        } //end DecodeString
    }
}
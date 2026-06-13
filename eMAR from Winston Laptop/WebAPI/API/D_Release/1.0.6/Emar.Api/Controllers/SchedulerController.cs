using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Emar.Api.Helpers;
using Emar.Core.Medications.Model;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Service;
using Emar.Core.Templates.Model;
using Emar.Core.Templates.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [Route("api/orders/schedulerOptions")]
    [ApiController]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class SchedulerController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITemplateService _templateService;

        public SchedulerController(IOrderService orderService, ITemplateService templateService)
        {
            _orderService = orderService;
            _templateService = templateService;
        }

        /// <summary>
        /// Retrieve scheduler options by brand name while looking at the formulary settings to filter the list of medications.
        /// </summary>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="siteId">Unique site identifier.</param>
        /// <param name="brandName">The name of a medication.</param>
        /// <returns></returns>
        [HttpGet("{brandName}/site/{siteId}", Name = nameof(GetSchedulerOptions))]
        [ProducesResponseType(typeof(SchedulerOptionsDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<SchedulerOptionsDto> GetSchedulerOptions(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "siteId")] int? siteId,
            [FromRoute(Name = "brandName")] string brandName
            )
        {
            //The only difference between this endpoint and the below endpoint is the /all.
            //This endpoint does take the formulary filters into account when filtering the list of medications being returned.
            //The one below does not look at the formulary settings (i.e. it does an "all" lookup)..
            //It was determined that the easiest thing to do was leave the existing endpoint alone and then
            //create a new endpoint for the "all" lookup.
            bool bAll = false;

            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest($"Site id is missing invalid.");
            }

            SchedulerOptionsDto ret;

            try
            {
                ret = _orderService.GetSchedulerSetupData(siteId.Value, DecodeString(brandName), bAll);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No setup options found for medication with brand name '{brandName}' for site with id '{siteId}'.");
            }

            return Ok(ret);
        }

        /// <summary>
        /// Retrieve scheduler options by brand name without applying any formulary filtering logic (i.e. doing an "all" lookup).
        /// </summary>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="siteId">Unique site identifier.</param>
        /// <param name="brandName">The name of a medication.</param>
        /// <returns></returns>
        [HttpGet("{brandName}/site/{siteId}/all", Name = nameof(GetSchedulerOptionsAll))]
        [ProducesResponseType(typeof(SchedulerOptionsDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<SchedulerOptionsDto> GetSchedulerOptionsAll(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "siteId")] int? siteId,
            [FromRoute(Name = "brandName")] string brandName
            )
        {
            //The only difference between this endpoint and the above endpoint is the /all.
            //This one does an "all" lookup to pull medications (i.e. it doesn't do any formulary filtering logic).
            //The one above this does do formulary filtering.
            //It was determined that the easiest thing to do was leave the existing endpoint alone and then
            //create a new endpoint for the "all" lookup.
            bool bAll = true;

            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest($"Site id is missing invalid.");
            }

            SchedulerOptionsDto ret;

            try
            {
                ret = _orderService.GetSchedulerSetupData(siteId.Value, DecodeString(brandName), bAll);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No setup options found for medication with brand name '{brandName}' for site with id '{siteId}'.");
            }

            return Ok(ret);
        }


        /// <summary>
        /// Retrieve scheduler options,
        /// </summary>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="siteId">Unique site identifier.</param>
        /// <param name="itemType">Order type (UserQuickListItem / DepartmentPreferredListItem / GroupRememberedOrder / PatientCartOrder / MedicationItem)</param>
        /// <param name="itemId">Unique order identifier.</param>
        /// <param name="duration">The duration of the future administrations.  Can be null.</param>
        /// <param name="durationUnitId">The ID of the unit fpr the future administrations (days, hours, minutes, etc...).  Can be null.</param>
        /// <param name="endDatetime">The end time for the future administrations.  Can be null.</param>
        /// <returns></returns>
        [HttpGet("{itemType}/{itemId}", Name = nameof(GetSchedulerOptionsListItem))]
        [ProducesResponseType(typeof(SchedulerOptionsDto), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<SchedulerOptionsDto> GetSchedulerOptionsListItem(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromRoute(Name = "itemType")] string itemType,
            [FromRoute(Name = "itemId")] int itemId,
            [FromQuery(Name = "duration")] int? duration,
            [FromQuery(Name = "durationUnitId")] int? durationUnitId,
            [FromQuery(Name = "end")] DateTimeOffset? endDatetime
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest($"Site id is missing.");
            }

            if (!Enum.TryParse(itemType.ToLower(), true, out EmarOrderType itemTypeEnum))
            {
                return BadRequest($"Item type '{itemType}' is invalid.");
            }

            SchedulerOptionsDto ret;

            try
            {
                ret = _orderService.GetSchedulerSetupData(siteId.Value, itemTypeEnum, itemId, duration, durationUnitId, endDatetime);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No setup options found for '{itemTypeEnum}' medication with id '{itemId}'.");
            }

            return Ok(ret);
        }

        [HttpGet("frequencies/site/{siteId}", Name = nameof(GetSchedulerFrequencies))]
        [ProducesResponseType(typeof(IEnumerable<FrequencyScheduleDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<FrequencyScheduleDto>> GetSchedulerFrequencies(
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
                return BadRequest($"Site id is missing.");
            }

            var ret = _orderService.GetFrequencies(siteId.Value);

            if (ret == null)
            {
                return NotFound($"No medication frequency schedules found for site with id '{siteId}'.");
            }

            return Ok(ret);
        }

        [HttpGet("routes/site/{siteId}", Name = nameof(GetSchedulerRoutes))]
        [ProducesResponseType(typeof(IEnumerable<MedicationRouteDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<MedicationRouteDto>> GetSchedulerRoutes(
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
                return BadRequest($"Site id is missing.");
            }

            IEnumerable<MedicationRouteDto> ret;

            try
            {
                ret = _orderService.GetRoutes(siteId.Value);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No medication routes found for site with id '{siteId}'.");
            }

            return Ok(ret);
        }

        [HttpGet("units/site/{siteId}", Name = nameof(GetSchedulerUnits))]
        [ProducesResponseType(typeof(IEnumerable<MedicationUnitDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<MedicationUnitDto>> GetSchedulerUnits(
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
                return BadRequest($"Site id is missing.");
            }

            IEnumerable<MedicationUnitDto> ret;

            try
            {
                ret = _orderService.GetUnits(siteId.Value);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No medication units found for site with id '{siteId}'.");
            }

            return Ok(ret);
        }

        /// <summary>
        /// Retrieve future administration events for a given frequency,
        /// </summary>
        /// <param name="mediaType">Media type from Accept header.</param>
        /// <param name="siteId">Unique site identifier.</param>
        /// <param name="frequencyId">The ID of the frequency</param>
        /// <param name="startDatetime">The start time for the future administrations.</param>
        /// <param name="endDatetime">The end time for the future administrations.  will normally be null.</param>
        /// <param name="duration">The duration of the future administrations.  Can be null.</param>
        /// <param name="durationUnitId">The ID of the unit fpr the future administrations (days, hours, minutes, etc...).  Can be null.</param>
        /// <returns></returns>
        [HttpGet("administrations/{frequencyId}", Name = nameof(GetSchedulerAdministrations))]
        [ProducesResponseType(typeof(IEnumerable<FrequencyScheduleAdministrationDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<FrequencyScheduleAdministrationDto>> GetSchedulerAdministrations(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromRoute(Name = "frequencyId")] int frequencyId,
            [FromQuery(Name = "start")] DateTimeOffset? startDatetime,
            [FromQuery(Name = "end")] DateTimeOffset? endDatetime,
            [FromQuery(Name = "duration")] int? duration,
            [FromQuery(Name = "durationUnitId")] int? durationUnitId

        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            IEnumerable<FrequencyScheduleAdministrationDto> ret;

            try
            {
                ret = _orderService.GetNewAdministrations(siteId.Value, frequencyId, startDatetime, endDatetime, duration, durationUnitId);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null || !ret.Any())
            {
                return NotFound($"No medication frequency schedule administrations found for frequency with id '{frequencyId}'.");
            }

            return Ok(ret);
        }

        [HttpGet("durationUnits", Name = nameof(GetSchedulerDurationUnits))]
        [ProducesResponseType(typeof(IEnumerable<DurationUnitDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<DurationUnitDto>> GetSchedulerDurationUnits(
            [FromHeader(Name = "Accept")] string mediaType
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            IEnumerable<DurationUnitDto> ret;

            try
            {
                ret = _orderService.GetDurationUnits();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, statusCode: (int)HttpStatusCode.InternalServerError);
            }

            if (ret == null)
            {
                return NotFound($"No duration units found.");
            }

            return Ok(ret);
        }

        private static string DecodeString(string encodedString)
        {
            //The band name comes in partially decoded.  Spaces are not %20 any more.
            //And the percent sign is a percent sign and not encoded (when the medication has one in the name).
            //But the forward slash was still %2f and not a forward slash.  This method fixes this.
            //Once we have the actual/decoded string, we're good.
            //The repository can find a match in the DB.
            //https://stackoverflow.com/a/3847593
            //Winston Murdock, 05/13/2021.  EMAR-1001.
            string decodedString;
            while ((decodedString = Uri.UnescapeDataString(encodedString)) != encodedString)
                encodedString = decodedString;
            return decodedString;
        }
    }
}
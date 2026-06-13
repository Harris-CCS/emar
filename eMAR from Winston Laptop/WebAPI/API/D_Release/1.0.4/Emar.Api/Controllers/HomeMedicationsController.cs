using System;
using Emar.Api.Helpers;
using Emar.Core.HomeMedications.Model;
using Emar.Core.HomeMedications.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    /// <summary>
    /// Patient Home Medications Controller
    /// </summary>
    [Route("api/homeMedications")]
    [ApiController]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class HomeMedicationsController : ControllerBase
    {
        private readonly IHomeMedicationService _homeMedicationService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="homeMedicationService"></param>
        public HomeMedicationsController(IHomeMedicationService homeMedicationService)
        {
            _homeMedicationService = homeMedicationService ?? throw new ArgumentNullException(nameof(homeMedicationService));
        }

        /// <summary>
        /// Get a a patient home medication.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="homeMedicationId">
        /// Unique patient home medication identifier.
        /// </param>
        /// <returns>An ActionResult of type HomeMedicationDto</returns>
        [HttpGet("{homeMedicationId}", Name = nameof(GetHomeMedication))]
        [ProducesResponseType(typeof(ActionResult<HomeMedicationDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<HomeMedicationDto> GetHomeMedication(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "homeMedicationId")] long homeMedicationId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var homeMed = _homeMedicationService.GetHomeMedication(homeMedicationId);

            if (homeMed == null)
            {
                return NotFound($"Patient home medication with id '{homeMedicationId}' was not found.");
            }

            return Ok(homeMed);
        }
    }
}
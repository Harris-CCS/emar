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
    public class DoseRangeCheckingInfoController : Controller
    {
        private readonly IDoseRangeCheckingInfoService _doseRangeCheckingInfoService;

        /// <summary>
        /// Constructor
        /// </summary>
        public DoseRangeCheckingInfoController(IDoseRangeCheckingInfoService service)
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
        }
    }
}

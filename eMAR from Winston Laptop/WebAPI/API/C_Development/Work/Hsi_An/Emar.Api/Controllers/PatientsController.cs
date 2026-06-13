using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Emar.Api.Helpers;
using Emar.Core.Helpers;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Service;
using Emar.Core.ResourceParameters;
using Emar.Data.Entities;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private Errors error = Errors.NoErrors;

        public PatientsController(IPatientService patientService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        /// <summary>
        /// Get a list of patients in the system.
        /// </summary>
        /// <param name="mediaType">
        /// Media type from Accept header.
        /// </param>
        /// <param name="userId">
        /// Unique user identifier.
        /// </param>
        /// <param name="orderBy">
        /// *Optional.* \
        /// Comma delimited list Patient element to sort by:
        /// * **Id**
        /// * **FullName**
        /// * **Age**
        /// * **DepartmentCode**
        /// * **WardCode**
        /// * **RoomBedCode**
        /// \
        /// \
        /// Optional **ASC** or **DESC** commands can be suffixed. \
        /// *Default:* **Id ASC**
        /// </param>
        /// <param name="fields">
        /// *Optional.* \
        /// Comma delimited list of patient elements to be returned.  If omitted, all patient elements are returned.
        /// </param>
        /// <param name="siteId">
        /// *Optional.* \
        /// Site (facility) identifier to restrict the list of returned patients to.
        /// </param>
        /// <param name="accountNumber">
        /// *Optional.* \
        /// Patient's account number.
        /// </param>
        /// <param name="customNumber">
        /// *Optional.* \
        /// Patient's custom number.
        /// </param>
        /// <param name="personNumber">
        /// *Optional.* \
        /// Patient's person number.
        /// </param>
        /// <param name="departmentCode">
        /// *Optional.* \
        /// Department code to restrict the list of returned patients to.
        /// </param>
        /// <param name="wardCodes">
        /// *Optional.* \
        /// Comma delimited of ward (area) codes to restrict the list of returned patients to.
        /// </param>
        /// <param name="roomBedCode">
        /// *Optional.* \
        /// Room and bed code to restrict the list of returned patients to.
        /// </param>
        /// <param name="includeMyPatientsOnly">
        /// *Optional.* \
        /// Include only the current user's patients in the list of returned patients.
        /// </param>
        /// <param name="includeInactive">
        /// *Optional.* \
        /// Include the inactive patients in the list of returned patients.
        /// </param>
        /// <param name="includeOrders">
        /// *Optional.* \
        /// Include the patients orders in the list of returned patients.
        /// </param>
        /// <param name="extId1">
        /// *Optional.* \
        /// First key of the external patient Id. In PulseCheck, it is the Site id.
        /// </param>
        /// <param name="extId2">
        /// *Optional.* \
        /// Second key of the external patient Id. In PulseCheck, it is the Ibex number.
        /// </param>
        /// <param name="pharmVerificationStatus">
        /// *Optional.* \
        /// 0, 1, 2 if we want to filter based on patients who have one, or morw,
        /// orders that have the PharmVerificationStatus field set to 0, 1, or 2.
        /// IF this is empty, we will not be filtering based on the PharmVerificationStatus.
        /// </param>
        /// <returns>An IActionResult of IEnumerable of type PatientDto</returns>
        [HttpGet(Name = nameof(GetPatients))]
        [ProducesResponseType(typeof(IEnumerable<PatientDto>), 200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<IEnumerable<PatientDto>> GetPatients(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromQuery] string orderBy,
            [FromQuery] string fields,
            [FromQuery] string accountNumber,
            [FromQuery] string customNumber,
            [FromQuery] string personNumber,
            [FromQuery] string departmentCode,
            [FromQuery] string wardCodes,
            [FromQuery] string roomBedCode,
            [FromQuery] string includeMyPatientsOnly,
            [FromQuery] string includeInactive,
            [FromQuery] string includeOrders,
            [FromQuery] string extId1,
            [FromQuery] string extId2,
            [FromQuery] string pharmacyVerificationStatus
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            var incMyPatientsOnly = bool.TryParse(includeMyPatientsOnly, out var value) && value;

            if (userId == null)
            {
                return BadRequest("User id is missing.");
            }

            var resourceParameters = new PatientsResourceParameters
            {
                SiteId = siteId ?? 0,
                UserId = userId.Value,
                OrderBy = orderBy,
                Fields = fields,
                AccountNumber = accountNumber,
                CustomNumber = customNumber,
                PersonNumber = personNumber,
                DepartmentCode = departmentCode,
                WardCodes = wardCodes,
                RoomBedCode = roomBedCode,
                IncludeMyPatientsOnly = incMyPatientsOnly,
                IncludeInactive = bool.TryParse(includeInactive, out bool incInactive) && incInactive,
                IncludeOrders = bool.TryParse(includeOrders, out bool incOrders) && incOrders,
                ExtId1 = extId1,
                ExtId2 = extId2,
                PharmacyVerificationStatus = pharmacyVerificationStatus
            };

            string orderLinkBase = null;
            string adminLinkBase = null;

            if (resourceParameters.IncludeOrders)
            {
                orderLinkBase = Url.Link(nameof(OrdersController.ExecuteOrderAction),
                        new { orderId = -99, actionId = "-98" })
                    .Replace("-99", "{0}")
                    .Replace("-98", "{1}");

                adminLinkBase = Url.Link(nameof(OrdersController.ExecuteAdministrationAction),
                        new { administrationId = -99, actionId = "-98" })
                    .Replace("-99", "{0}")
                    .Replace("-98", "{1}");
            }

            if (resourceParameters.AskingForLegacyPulseCheckPatient())
            {
                if (extId1 != null)
                {
                    var pt = _patientService.GetPatient(short.Parse(extId1), extId2, resourceParameters.IncludeOrders,
                        orderLinkBase, adminLinkBase, userId.Value);

                    if (pt == null)
                    {
                        return NotFound($"Patient with site '{extId1}' and ibex '{extId2}' was not found.");
                    }

                    return Ok(pt);
                }
            }

            if (resourceParameters.AskingForPatientByAccountNumber())
            {
                var pt = _patientService.GetPatientByNumber(accountNumber, GetPatientBy.AccountNumber,
                    resourceParameters.IncludeOrders, adminLinkBase, orderLinkBase, userId.Value);

                if (pt == null)
                {
                    return NotFound($"Patient with Account Number '{accountNumber}' was not found.");
                }

                return Ok(pt);
            }

            if (resourceParameters.AskingForPatientByCustomNumber())
            {
                var pt = _patientService.GetPatientByNumber(customNumber, GetPatientBy.CustomNumber,
                    resourceParameters.IncludeOrders, adminLinkBase, orderLinkBase, userId.Value);

                if (pt == null)
                {
                    return NotFound($"Patient with Custom Number '{customNumber}' was not found.");
                }

                return Ok(pt);
            }

            if (resourceParameters.AskingForPatientByPersonNumber())
            {
                var pt = _patientService.GetPatientByNumber(personNumber, GetPatientBy.PersonNumber,
                    resourceParameters.IncludeOrders, adminLinkBase, orderLinkBase, userId.Value);

                if (pt == null)
                {
                    return NotFound($"Patient with Person Number '{personNumber}' was not found");
                }

                return Ok(pt);
            }

            if (!_propertyMappingService.ValidMappingExistsFor<PatientDto, Patient>(orderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientDto>(fields))
            {
                return BadRequest();
            }

            var patients = _patientService.GetPatients(resourceParameters, false);

            if (patients == null)
            {
                return NotFound($"No patients found.");
            }

            var paginationMetadata = new
            {
                totalCount = patients.TotalCount,
                pageSize = patients.PageSize,
                currentPage = patients.CurrentPage,
                totalPages = patients.TotalPages
            };

            Response.Headers.Add("EMAR-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateHateOasLinksForPatients(resourceParameters, patients.HasNext, patients.HasPrevious);
            var shapedPatients = ((IEnumerable<PatientDto>)patients).ShapeData(fields);

            var shapedPatientsWithLinks = shapedPatients.Select(patient =>
            {
                var patientAsDictionary = patient as IDictionary<string, object>;
                var patientLinks = CreateHateOasLinksForPatient((long)patientAsDictionary["Id"], resourceParameters);

                patientAsDictionary.Add("links", patientLinks);

                return patientAsDictionary;
            });

            var linkedPatientResource = new
            {
                patients = shapedPatientsWithLinks,
                links
            };

            return Ok(linkedPatientResource);
        }

        [HttpGet("{patientId}", Name = nameof(GetPatient))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<PatientDto> GetPatient(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "patientId")] long patientId,
            [FromQuery] PatientsResourceParameters resourceParameters
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var patient = CheckPatient(patientId, resourceParameters, false);

            if (error.Equals(Errors.BadRequest))
            {
                return BadRequest();
            }

            if (error.Equals(Errors.PatientNotFound) || (patient == null))
            {
                return NotFound($"Patient with id '{patientId}' was not found.");
            }

            var links = CreateHateOasLinksForPatient(patientId, resourceParameters);
            var linkedResourceToReturn = patient.ShapeData(resourceParameters.Fields) as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpGet("{patientId}/orders", Name = nameof(GetPatientOrders))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<PatientDto> GetPatientOrders(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-User")] int? userId,
            [FromRoute(Name = "patientId")] long patientId
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

            var resourceParameters = new PatientsResourceParameters
            {
                UserId = userId.Value
            };

            // Get the order and administration base for the Action links
            var orderLinkBase = Url.Link(nameof(OrdersController.ExecuteOrderAction),
                    new { orderId = -99, actionId = "-98" })
                .Replace("-99", "{0}")
                .Replace("-98", "{1}");

            var administrationLinkBase = Url.Link(nameof(OrdersController.ExecuteAdministrationAction),
                    new { administrationId = -99, actionId = "-98" })
                .Replace("-99", "{0}")
                .Replace("-98", "{1}");

            var patient = CheckPatient(patientId, resourceParameters, true, orderLinkBase, administrationLinkBase);

            if (error.Equals(Errors.BadRequest))
            {
                return BadRequest();
            }

            if (error.Equals(Errors.PatientNotFound) || (patient == null))
            {
                return NotFound($"Patient with id '{patientId}' was not found.");
            }

            return Ok(patient);
        }

        /* Commenting out the following call - doesn't use orderId, and dosen't really make sense,
         * so more prudent to comment it out than to fix it */
        //[HttpGet("{patientId}/orders/{orderId}", Name = nameof(GetPatientOrder))]
        //[ProducesResponseType(200)] // (OK) - the resource is sent in the response
        //[ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        //[ProducesResponseType(404)] // (not found) - the resource does not exits
        //[ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        //public IActionResult GetPatientOrder(
        //    [FromHeader(Name = "Accept")] string mediaType,
        //    [FromQuery] PatientsResourceParameters resourceParameters,
        //    long? patientId,
        //    long orderId
        //    )
        //{
        //    if (!MediaTypes.IsValidMediaType(mediaType))
        //    {
        //        return BadRequest("Unsupported media type header provided.");
        //    }

        //    PatientDto patient = CheckPatient(patientId, null, true);

        //    if (error.Equals(Errors.BadRequest))
        //    {
        //        return BadRequest();
        //    }

        //    if (error.Equals(Errors.PatientNotFound) || (patient == null))
        //    {
        //        return NotFound($"Patient with id '{patientId}' was not found");
        //    }

        //    return Ok(patient);
        //}

        private PatientDto CheckPatient(long? patientId, PatientsResourceParameters resourceParameters,
            bool includeOrders, string orderLinkBase = null, string administrationLinkBase = null)
        {
            if ((patientId == null) &&
                (resourceParameters.ExtId1 == null) &&
                (resourceParameters.ExtId2 == null))
            {
                error = Errors.BadRequest;
            }

            var patient = _patientService.GetPatient((long)patientId, resourceParameters, includeOrders,
                orderLinkBase, administrationLinkBase);

            if (patient == null)
            {
                error = Errors.PatientNotFound;
            }

            return patient;
        }

        private string CreatePatientsResourceUri(PatientsResourceParameters resourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        resourceParameters.PageNumber = resourceParameters.PageNumber - 1;
                        return Url.Link(nameof(GetPatients), resourceParameters);
                    }
                case ResourceUriType.NextPage:
                    {
                        resourceParameters.PageNumber = resourceParameters.PageNumber + 1;
                        return Url.Link(nameof(GetPatients), resourceParameters);
                    }
                case ResourceUriType.Current:
                default:
                    {
                        return Url.Link(nameof(GetPatients), resourceParameters);
                    }
            }
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForPatient(long? patientId, [FromQuery] PatientsResourceParameters resourceParameters)
        {
            var links = new List<HateOasLinkDto>();

            if (string.IsNullOrWhiteSpace(resourceParameters.Fields))
            {
                links.Add(
                    new HateOasLinkDto(Url.Link(nameof(GetPatient), new { patientId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new HateOasLinkDto(Url.Link(nameof(GetPatient), new { patientId, resourceParameters.Fields }),
                    "self",
                    "GET"));
            }

            //links.Add(
            //    new HateOasLinkDto(Url.Link(nameof(CreatePatientOrder), new { patientId }),
            //    "create_patient_order",
            //    "POST"));

            return links;
        }

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForPatients([FromQuery] PatientsResourceParameters resourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<HateOasLinkDto>();

            links.Add(
                new HateOasLinkDto(CreatePatientsResourceUri(resourceParameters, ResourceUriType.Current),
                "self",
                "GET"));

            if (hasNext)
            {
                links.Add(
                    new HateOasLinkDto(CreatePatientsResourceUri(resourceParameters, ResourceUriType.NextPage),
                    "nextPage",
                    "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new HateOasLinkDto(CreatePatientsResourceUri(resourceParameters, ResourceUriType.PreviousPage),
                    "previousPage",
                    "GET"));
            }

            return links;
        }

        private enum Errors
        {
            NoErrors = 0,
            BadRequest = 1,
            PatientNotFound = 2
        }
    }
}
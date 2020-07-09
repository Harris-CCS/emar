using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using Emar.Core;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Service;
using Emar.Data.Entities;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class PatientsController : ControllerBase
    {
        private readonly EmarContext _context;
        private readonly IPatientService _patientService;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
        private Errors error = Errors.NoErrors;

        public PatientsController(EmarContext emarContext,
                                  IPatientService patientService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _context = emarContext;
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
        /// <returns>An IActionResult of IEnumerable of type PatientDto</returns>
        [HttpGet(Name = nameof(GetPatients))]
        [HttpHead]
        public ActionResult<IEnumerable<PatientDto>> GetPatients(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string orderBy,
            [FromQuery] string fields,
            [FromQuery] short? siteId,
            [FromQuery] string departmentCode,
            [FromQuery] string wardCodes,
            [FromQuery] string roomBedCode,
            [FromQuery] string includeInactive,
            [FromQuery] string includeOrders,
            [FromQuery] string extId1,
            [FromQuery] string extId2
            )
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            PatientsResourceParameters resourceParameters = new PatientsResourceParameters
            {
                OrderBy = orderBy,
                Fields = fields,
                SiteId = siteId,
                DepartmentCode = departmentCode,
                WardCodes = wardCodes,
                RoomBedCode = roomBedCode,
                IncludeInactive = bool.TryParse(includeInactive, out bool incInactive) ? incInactive : false,
                IncludeOrders = bool.TryParse(includeOrders, out bool incOrders) ? incInactive : false,
                ExtId1 = extId1,
                ExtId2 = extId2
            };

            if (resourceParameters.AskingForLegacyPulseCheckPatient())
            {
                if (extId1 != null)
                {
                    PatientDto pt = _patientService.GetPatient(short.Parse(extId1), extId2);

                    if (pt == null) { return NotFound($"Patient with site: {extId1} and ibex: {extId2} was not found"); }

                    return Ok(pt);
                }
            }

            if (!_propertyMappingService.ValidMappingExistsFor<PatientDto, Patient>(orderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientDto>(fields))
            {
                return BadRequest();
            }

            PagedList<PatientDto> patients = _patientService.GetPatients(resourceParameters, false);

            if (patients == null) { return NotFound($"No patients found"); }

            var paginationMetadata = new
            {
                totalCount = patients.TotalCount,
                pageSize = patients.PageSize,
                currentPage = patients.CurrentPage,
                totalPages = patients.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            if (parsedMediaType.MediaType.Equals(MediaTypes.PcEmar))
            {
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

            return Ok(((IEnumerable<PatientDto>)patients).ShapeData(fields));
        }

        [HttpGet("{patientId}", Name = nameof(GetPatient))]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public ActionResult<PatientDto> GetPatient(long? patientId, [FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
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

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            if (parsedMediaType.MediaType.Equals(MediaTypes.PcEmar))
            {
                var links = CreateHateOasLinksForPatient(patientId, resourceParameters);
                var linkedResourceToReturn = patient.ShapeData(resourceParameters.Fields) as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return Ok(linkedResourceToReturn);
            }

            return Ok(patient.ShapeData(resourceParameters.Fields));
        }

        [HttpGet("{patientId}/orders", Name = nameof(GetPatientOrders))]
        public IActionResult GetPatientOrders(long? patientId, [FromQuery] PatientsResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            PatientDto patient = CheckPatient(patientId, null, true);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            return Ok(patient);
        }

        [HttpGet("{patientId}/orders/{orderId}", Name = nameof(GetPatientOrder))]
        public IActionResult GetPatientOrder(long? patientId, long orderId, [FromQuery] PatientsResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            PatientDto patient = CheckPatient(patientId, null, true);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            return Ok(patient);
        }

        private PatientDto CheckPatient(long? patientId, PatientsResourceParameters resourceParameters, bool includeOrders)
        {
            if ((patientId == null) &&
                (resourceParameters.ExtId1 == null) &&
                (resourceParameters.ExtId2 == null))
            {
                error = Errors.BadRequest;
            }

            PatientDto patient = _patientService.GetPatient((long)patientId, resourceParameters, includeOrders);

            if (patient == null) { error = Errors.PatientNotFound; }

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
            List<HateOasLinkDto> links = new List<HateOasLinkDto>();

            if (String.IsNullOrWhiteSpace(resourceParameters.Fields))
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

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForPatients([FromQuery] ResourceParameters resourceParameters, bool hasNext, bool hasPrevious)
        {
            List<HateOasLinkDto> links = new List<HateOasLinkDto>
            {
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
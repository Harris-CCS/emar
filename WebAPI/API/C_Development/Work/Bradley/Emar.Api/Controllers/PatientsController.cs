using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Emar.Core;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Service;
using Emar.Data;
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

        [HttpGet(Name = nameof(GetPatients))]
        [HttpHead]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetPatients([FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (resourceParameters.AskingForLegacyPulseCheckPatient())
            {
                if (resourceParameters.Site != null)
                {
                    PatientDto pt =
                        _patientService.GetPatient((short) resourceParameters.Site, resourceParameters.Ibex);

                    if(pt == null) { return NotFound($"Patient with site: {resourceParameters.Site} and ibex: {resourceParameters.Ibex} was not found"); }

                    return Ok(pt);
                }
            }

            if (!_propertyMappingService.ValidMappingExistsFor<PatientDto, Patient>(resourceParameters.OrderBy))
            {
               return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientDto>(resourceParameters.Fields))
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
                var shapedPatients = ((IEnumerable<PatientDto>)patients).ShapeData(resourceParameters.Fields);

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

            return Ok(((IEnumerable<PatientDto>)patients).ShapeData(resourceParameters.Fields));
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
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetPatientOrders(long? patientId, [FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            patientId ??= resourceParameters.PatientId ?? null;

            PatientDto patient = CheckPatient(patientId, null, true);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            return Ok(patient);
        }

        [HttpGet("{patientId}/orders/{orderId}", Name = nameof(GetPatientOrder))]
        [Produces(MediaTypes.PcEmar, MediaTypes.Json)]
        public IActionResult GetPatientOrder(long? patientId, long orderId, [FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            patientId ??= resourceParameters.PatientId ?? null;

            PatientDto patient = CheckPatient(patientId, null, true);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            return Ok(patient);
        }

        private PatientDto CheckPatient(long? patientId, ResourceParameters resourceParameters, bool includeOrders)
        {
            patientId ??= resourceParameters.PatientId ?? null;

            if ((patientId == null) &&
                (resourceParameters.Site == null) &&
                (resourceParameters.Ibex == null))
            {
                error = Errors.BadRequest;
            }

            PatientDto patient = _patientService.GetPatient((long)patientId, resourceParameters, includeOrders);

            if (patient == null) { error = Errors.PatientNotFound; }

            return patient;
        }

        private string CreatePatientsResourceUri(ResourceParameters resourceParameters, ResourceUriType type)
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

        private IEnumerable<HateOasLinkDto> CreateHateOasLinksForPatient(long? patientId, [FromQuery] ResourceParameters resourceParameters)
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
                    "GET")
            };

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
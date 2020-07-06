using System;
using System.Collections.Generic;
using System.Linq;
#if PAGING || SORTING || EXPANDO
using System.Text.Json;
#endif
using Emar.Core;
using Emar.Core.Orders.Model;
using Emar.Core.Orders.Repository;
using Emar.Core.Patients.Model;
using Emar.Core.Patients.Service;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Emar.Api.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly EmarContext _context;
#if PAGING || SORTING || EXPANDO
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;
#endif
        private Errors error = Errors.NoErrors;

#if ORIGINAL
        public PatientsController(EmarContext emarContext, IPatientService patientService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _context = emarContext;
        }
#endif
#if PAGING || SORTING
        public PatientsController(EmarContext emarContext,
                                  IPatientService patientService,
                                  IPropertyMappingService propertyMappingService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _context = emarContext;
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }
#endif
#if EXPANDO
        public PatientsController(EmarContext emarContext,
                                  IPatientService patientService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _context = emarContext;
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }
#endif

#if ORIGINAL
        [HttpGet(Name = nameof(GetPatients))]
        [HttpHead]
        [Produces(typeof(IEnumerable<PatientDto>))]
        public IActionResult GetPatients([FromQuery] ResourceParameters resourceParameters)
        {
            var patients = _patientService.GetPatients(resourceParameters);

            if (patients == null) { return NotFound($"No patients found"); }

            return Ok(patients);
        }
#endif
#if PAGING || SORTING || EXPANDO
        [HttpGet(Name = nameof(GetPatients))]
        [HttpHead]
        [Produces(typeof(IEnumerable<PatientDto>))]
        public IActionResult GetPatients([FromQuery] ResourceParameters resourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<PatientDto, Patient>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            PagedList<PatientDto> patients = _patientService.GetPatients(resourceParameters);

            if (patients == null) { return NotFound($"No patients found"); }

            var paginationMetadata = new
            {
                totalCount = patients.TotalCount,
                pageSize = patients.PageSize,
                currentPage = patients.CurrentPage,
                totalPages = patients.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

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
                value = shapedPatientsWithLinks,
                links
            };

            return Ok(linkedPatientResource);
        }
#endif


#if ORIGINAL
        [HttpGet("{patientId}", Name = nameof(GetPatient))]
        [Produces(typeof(PatientDto))]
        public IActionResult GetPatient(long? patientId, [FromQuery] ResourceParameters resourceParameters)
        {
            var patient = CheckPatient(patientId, resourceParameters);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            return Ok(patient);
        }
#endif
#if PAGING || SORTING || EXPANDO
        [HttpGet("{patientId}", Name = nameof(GetPatient))]
        //[Produces(typeof(PatientDto))]
        [Produces("application/json",
            MediaTypes.PcEmarMediaType)]
        public IActionResult GetPatient(long? patientId, [FromQuery] ResourceParameters resourceParameters, [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<PatientDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var patient = CheckPatient(patientId, resourceParameters);

            if (error.Equals(Errors.BadRequest))
            {
                return BadRequest();
            }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            if (parsedMediaType.MediaType.Equals(MediaTypes.PcEmarMediaType))
            {
                var links = CreateHateOasLinksForPatient(patientId, resourceParameters);
                var linkedResourceToReturn = patient.ShapeData(resourceParameters.Fields) as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return Ok(linkedResourceToReturn);
            }

            return Ok(patient.ShapeData(resourceParameters.Fields));
        }
#endif
        [HttpGet("{patientId}/orders", Name = nameof(GetPatientOrders))]
        [Produces(typeof(IEnumerable<OrderDto>))]
        public IActionResult GetPatientOrders(long? patientId, [FromQuery] ResourceParameters resourceParameters)
        {
            patientId ??= resourceParameters.PatientId ?? null;

            PatientDto patient = CheckPatient(patientId, null);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            OrderRepository orderRepository = new OrderRepository(_context);
            IEnumerable<Order> orders = orderRepository.GetOrders(patientId, resourceParameters);

            if (resourceParameters.IncludePatient.Equals(true) &&
                (orders != null))
            {
                patient.Orders ??= Enumerable.Empty<Order>();
                patient.Orders = orders;

                return Ok(patient);
            }

            return Ok(orders);
        }

        [HttpGet("{patientId}/orders/{orderId}", Name = nameof(GetPatientOrder))]
        [Produces(typeof(IEnumerable<OrderDto>))]
        public IActionResult GetPatientOrder(long? patientId, long orderId, [FromQuery] ResourceParameters resourceParameters)
        {
            patientId ??= resourceParameters.PatientId ?? null;

            PatientDto patient = CheckPatient(patientId, null);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            OrderRepository orderRepository = new OrderRepository(_context);
            Order order = orderRepository.GetOrder(orderId, resourceParameters);

            if (resourceParameters.IncludePatient.Equals(true) &&
                (order != null))
            {
                patient.Orders ??= Enumerable.Empty<Order>();
                patient.Orders = new[] { order };

                return Ok(patient);
            }

            return Ok(order);
        }

        private PatientDto CheckPatient(long? patientId, ResourceParameters resourceParameters)
        {
            patientId ??= resourceParameters.PatientId ?? null;

            if ((patientId == null) &&
                (resourceParameters.Site == null) &&
                (resourceParameters.Ibex == null))
            {
                error = Errors.BadRequest;
            }

            PatientDto patient = _patientService.GetPatient((long)patientId, resourceParameters);

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

        public IEnumerable<HateOasLinkDto> CreateHateOasLinksForPatient(long? patientId, [FromQuery] ResourceParameters resourceParameters)
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

        public IEnumerable<HateOasLinkDto> CreateHateOasLinksForPatients([FromQuery] ResourceParameters resourceParameters, bool hasNext, bool hasPrevious)
        {
            List<HateOasLinkDto> links = new List<HateOasLinkDto>();

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

        public enum Errors
        {
            NoErrors = 0,
            BadRequest = 1,
            PatientNotFound = 2
        }
    }
}
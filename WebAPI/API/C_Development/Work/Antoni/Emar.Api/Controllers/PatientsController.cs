using System;
using System.Collections.Generic;
using System.Linq;
#if PAGING || SORTING
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

namespace Emar.Api.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly EmarContext _context;
        private Errors error = Errors.NoErrors;

        public PatientsController(EmarContext emarContext, IPatientService patientService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _context = emarContext;

        }

#if ORIGINAL
        [HttpGet(Name = nameof(GetPatients))]
        [Produces(typeof(IEnumerable<PatientDto>))]
        public IActionResult GetPatients([FromQuery] ResourceParameters resourceParameters)
        {
            var patients = _patientService.GetPatients(resourceParameters);

            if (patients == null) { return NotFound($"No patients found"); }

            return Ok(patients);
        }
#endif
#if PAGING || SORTING
        public IActionResult GetPatients([FromQuery] ResourceParameters resourceParameters)
        {
            var patients = _patientService.GetPatients(resourceParameters);

            if (patients == null) { return NotFound($"No patients found"); }

#region Paging
            var previousPageLink = patients.HasPrevious ? CreatePatientsResourceUri(resourceParameters, ResourceUriType.PreviousPage) : null;
            var nextPageLink = patients.HasNext ? CreatePatientsResourceUri(resourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = patients.TotalCount,
                pageSize = patients.PageSize,
                currentPage = patients.CurrentPage,
                totalPages = patients.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
#endregion

            return Ok(patients);
        }
#endif

        [HttpGet("{patientId}", Name = nameof(GetPatient))]
        [Produces(typeof(PatientDto))]
        public IActionResult GetPatient(long? patientId, [FromQuery] ResourceParameters resourceParameters)
        {
            var patient = CheckPatient(patientId, resourceParameters);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            return Ok(patient);
        }

        [HttpGet("{patientId}/orders", Name = nameof(GetPatientOrders))]
        [Produces(typeof(IEnumerable<OrderDto>))]
        public IActionResult GetPatientOrders(long? patientId, [FromQuery] ResourceParameters resourceParameters)
        {
            patientId ??= resourceParameters.PatientId ?? null;

            var patient = CheckPatient(patientId, null);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            var orderRepository = new OrderRepository(_context);
            var orders = orderRepository.GetOrders(patientId, resourceParameters);

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

            var patient = CheckPatient(patientId, null);

            if (error.Equals(Errors.BadRequest)) { return BadRequest(); }

            if (error.Equals(Errors.PatientNotFound) || (patient == null)) { return NotFound($"Patient with id {patientId} was not found"); }

            var orderRepository = new OrderRepository(_context);
            var order = orderRepository.GetOrder(orderId, resourceParameters);

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

#if PAGING || SORTING
       private string CreatePatientsResourceUri(ResourceParameters resourceParameters, ResourceUriType type)
        {
            return type switch
            {
                ResourceUriType.PreviousPage => Url.Link(
                    nameof(GetPatients),
                    new
                    {
                        PageNumber = resourceParameters.PageNumber - 1,
                        resourceParameters.PageSize,
                                                             
                        resourceParameters.DepartmentCode,
                        resourceParameters.Ibex,
                        resourceParameters.IncludeAdministrations,
                        resourceParameters.IncludeAdministrationsEvents,
                        resourceParameters.IncludeInactive,
                        resourceParameters.IncludePatient,
                        resourceParameters.PatientId,
                        resourceParameters.Site
                    }),
                ResourceUriType.NextPage => Url.Link(
                    nameof(GetPatients),
                    new
                    {
                        PageNumber = resourceParameters.PageNumber + 1,
                        resourceParameters.PageSize,

                        resourceParameters.DepartmentCode,
                        resourceParameters.Ibex,
                        resourceParameters.IncludeAdministrations,
                        resourceParameters.IncludeAdministrationsEvents,
                        resourceParameters.IncludeInactive,
                        resourceParameters.IncludePatient,
                        resourceParameters.PatientId,
                        resourceParameters.Site
                    }),
                _ => Url.Link(
                    nameof(GetPatients),
                    new
                    {
                        resourceParameters.PageNumber,
                        resourceParameters.PageSize,

                        resourceParameters.DepartmentCode,
                        resourceParameters.Ibex,
                        resourceParameters.IncludeAdministrations,
                        resourceParameters.IncludeAdministrationsEvents,
                        resourceParameters.IncludeInactive,
                        resourceParameters.IncludePatient,
                        resourceParameters.PatientId,
                        resourceParameters.Site
                    }),
            };
        }
#endif

        public enum Errors
        {
            NoErrors = 0,
            BadRequest = 1,
            PatientNotFound = 2
        }
    }
}
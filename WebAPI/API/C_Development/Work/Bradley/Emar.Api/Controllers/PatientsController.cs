using System;
using Emar.Core.Patients.Service;
using Microsoft.AspNetCore.Mvc;


namespace Emar.Api.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService ??
                                 throw new ArgumentNullException(nameof(patientService));
        }

        [HttpGet]
        public IActionResult GetPatients([FromQuery] bool activeOnly, [FromQuery] int siteId, [FromQuery] string deptCode)
        {
            var patients = _patientService.GetPatients(activeOnly, siteId);
            if (patients == null)
            {
                return NotFound();
            }

            return Ok(patients);
        }

        [HttpGet]
        //public IActionResult GetPatient([FromQuery] int site, [FromQuery] string ibex)
        //{
        //    long patientId = _patientService.GetPatientIdFromPulseCheck(site, ibex);

        //    return GetPatient(patientId);
        //}

        [HttpGet("{patientId}")]
        public IActionResult GetPatient(long patientId)
        {
            var patient = _patientService.GetPatient(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }
    }
}
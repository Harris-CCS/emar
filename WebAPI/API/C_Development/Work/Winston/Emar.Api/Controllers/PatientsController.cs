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

        [HttpGet("{patientId}")]
        public IActionResult GetPatient(int patientId)
        {
            var patient = _patientService.GetPatient(patientId);
            return new JsonResult(patient);
        }
    }
}
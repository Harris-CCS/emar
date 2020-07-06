using System.Collections.Generic;
using Emar.Core;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class RootController : ControllerBase
    {
        [HttpGet("About")]
        public ContentResult About()
        {
            return Content("An Electronic Medication Administration Record API.");
        }

        [HttpGet("version")]
        public string Version()
        {
            return "Version 0.0.1";
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            List<HateOasLinkDto> links = new List<HateOasLinkDto>();

            links.Add(
                new HateOasLinkDto(Url.Link(nameof(GetRoot), new { }),
                "self",
                "GET"));

            links.Add(
                new HateOasLinkDto(Url.Link(nameof(PatientsController.GetPatients), new { }),
                "patients",
                "GET"));

            links.Add(
                new HateOasLinkDto(Url.Link(nameof(OrdersController.GetOrders), new { }),
                "orders",
                "GET"));

            //links.Add(
            //    new HateOasLinkDto(Url.Link(nameof(PatientsController.CreatePatientOrder), new { patientId }),
            //    "create_patient_order",
            //    "POST"));

            return Ok(links);
        }
    }
}

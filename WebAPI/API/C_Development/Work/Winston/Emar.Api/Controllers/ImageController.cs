using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Emar.Api.Helpers;
using Emar.Core.Helpers;
using Emar.Core.Patients.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using static Emar.Core.Patients.Model.Constants;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route(AppConstants.ImagesRoute)]
    public class ImageController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public ImageController(IPatientService patientService)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        }

        [HttpGet("patients", Name = nameof(GetPatientImage))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
        [HttpCacheValidation(MustRevalidate = true)]
        [Produces(MediaTypes.Jpeg, MediaTypes.Text)]
        public IActionResult GetPatientImage(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string getPatientBy,
            [FromQuery] string number
            )
        {
            if ((getPatientBy == null) ||
                (number == null))
            {
                return BadRequest();
            }

            GetPatientBy patientBy = GetPatientBy.None;

            switch (getPatientBy.ToLower())
            {
                case "id":
                    patientBy = GetPatientBy.Id;
                    break;
                case "medicalrecordnumber":
                    patientBy = GetPatientBy.MedicalRecordNumber;
                    break;
                case "accountnumber":
                    patientBy = GetPatientBy.AccountNumber;
                    break;
                case "customnumber":
                    patientBy = GetPatientBy.CustomNumber;
                    break;
                case "personnumber":
                    patientBy = GetPatientBy.PersonNumber;
                    break;
                default:
                    break;
            }

            if (patientBy == GetPatientBy.None)
            {
                return BadRequest();
            }

            string error;
            PhysicalFileResult physicalFileResult = GetPatientImageFile(number, patientBy, out error);

            if (error.StartsWith("BadRequest|"))
            {
                return BadRequest();
            }

            if (error.StartsWith("NotFound|"))
            {
                return NotFound(error.Split("|")[1]);
            }

            return physicalFileResult;
        }

        [HttpGet("patients/{patientId}", Name = nameof(GetPatientImageById))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
        [HttpCacheValidation(MustRevalidate = true)]
        [Produces(MediaTypes.Jpeg, MediaTypes.Text)]
        public IActionResult GetPatientImageById(
            [FromHeader(Name = "Accept")] string mediaType,
            long? patientId
            )
        {
            if (patientId == null)
            {
                return BadRequest();
            }

            string error;
            PhysicalFileResult physicalFileResult = GetPatientImageFile(patientId.ToString(), GetPatientBy.Id, out error);

            if (error.StartsWith("BadRequest|"))
            {
                return BadRequest();
            }

            if (error.StartsWith("NotFound|"))
            {
                return NotFound(error.Split("|")[1]);
            }

            return physicalFileResult;
        }

        private PhysicalFileResult GetPatientImageFile(string number, GetPatientBy getPatientBy, out string error)
        {
            error = "";

            Dictionary<string, string> externalRootSitePatientId = _patientService.GetExternalRootSitePatientId(number, getPatientBy);

            if (externalRootSitePatientId == null)
            {
                error = "BadRequest|";
                return null;
            }

            var root = externalRootSitePatientId["root"];
            var site = externalRootSitePatientId["siteId"];
            var patientId = externalRootSitePatientId["patientId"];

            if ((root == null) ||
                (site == null) ||
                (patientId == null))
            {
                error = "BadRequest|";
                return null;
            }

            DateTime.TryParseExact(patientId, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtPatientId);

            //"<root>/inc/<site_id>/nct/<YYYY from ibex>/<MM from ibex>/<ibex>.jpg"
            var filepath = root + @"\" +
                            site + @"\" +
                            @"nct\" +
                            dtPatientId.ToString(@"yyyy") + @"\" +
                            dtPatientId.ToString(@"MM") + @"\" +
                            dtPatientId.ToString(@"yyyyMMddHHmmss") + @".jpg";


            //var filepath = @"\\ros-57c-dx01.picis.com\E$\ibex\inc\1\nct\2019\05\" + extId + @".jpg";

            FileInfo fInfo = new FileInfo(filepath);

            if (!fInfo.Exists)
            {
                error = $"NotFound|Image {fInfo.Name} not found.";
                return null;
            }

            return PhysicalFile(filepath, MediaTypes.Jpeg);
        }
    }
}

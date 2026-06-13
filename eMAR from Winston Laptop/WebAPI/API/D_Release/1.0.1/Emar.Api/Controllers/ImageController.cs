using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Emar.Api.Helpers;
using Emar.Core.Helpers;
using Emar.Core.Options.Model;
using Emar.Core.Patients.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        [Produces(MediaTypes.Jpeg, MediaTypes.Png, MediaTypes.Gif, MediaTypes.Text)]
        public IActionResult GetPatientImage(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string getPatientBy,
            [FromQuery] string number,
            [FromQuery] bool detailsOnError = false
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
            PhysicalFileResult physicalFileResult = GetPatientImageFile(number, patientBy, out error, detailsOnError);

            if (error.StartsWith("BadRequest|"))
            {
                return BadRequest(error.Split("|")[1]);
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
        [Produces(MediaTypes.Jpeg, MediaTypes.Png, MediaTypes.Gif, MediaTypes.Text)]
        public IActionResult GetPatientImageById(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "patientId")] long? patientId,
            [FromQuery] bool detailsOnError = false
            )
        {
            if (patientId == null)
            {
                return BadRequest();
            }

            string error;
            PhysicalFileResult physicalFileResult = GetPatientImageFile(patientId.ToString(), GetPatientBy.Id, out error, detailsOnError);

            if (error.StartsWith("BadRequest|"))
            {
                return BadRequest(error.Split("|")[1]);
            }

            if (error.StartsWith("NotFound|"))
            {
                return NotFound(error.Split("|")[1]);
            }

            return physicalFileResult;
        }

        [HttpGet("patients/{patientId}/indicators/{imageName}", Name = nameof(GetPatientIndicatorImage))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
        [HttpCacheValidation(MustRevalidate = true)]
        [Produces(MediaTypes.Jpeg, MediaTypes.Png, MediaTypes.Gif, MediaTypes.Text)]
        public IActionResult GetPatientIndicatorImage(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "patientId")] long? patientId,
            [FromRoute(Name = "imageName")] string imageName,
            [FromQuery] bool detailsOnError = false
            )
        {
            if (imageName == null)
            {
                return BadRequest();
            }

            string error;
            PhysicalFileResult physicalFileResult = GetPatientIndicatorImageFile(patientId.ToString(), GetPatientBy.Id, imageName, out error, detailsOnError);

            if (error.StartsWith("BadRequest|"))
            {
                return BadRequest(error.Split("|")[1]);
            }

            if (error.StartsWith("NotFound|"))
            {
                return NotFound(error.Split("|")[1]);
            }

            return physicalFileResult;
        }

        private PhysicalFileResult GetPatientImageFile(string number, GetPatientBy getPatientBy, out string error, bool detailsOnError = false)
        {
            error = "";

            Dictionary<string, string> externalRootSitePatientId = _patientService.GetExternalRootSitePatientId(number, getPatientBy, OptionNames.PATIENT_IMAGE_PATH.ToString());

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
                error = "BadRequest|"
                        + (detailsOnError ? Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            ">>ImageController 0001<<" + Environment.NewLine +
                                            "------------------------" + Environment.NewLine +
                                            "root: [" + root + "]" + Environment.NewLine +
                                            "site: [" + site + "]" + Environment.NewLine +
                                            "patientId: [" + patientId + "]" + Environment.NewLine 
                                          : "");
                return null;
            }

            DateTime.TryParseExact(patientId, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtPatientId);

            //var filepath = @"\\ros-57c-dx01.picis.com\E$\ibex\inc\1\nct\2019\05\20190517055504.jpg";
            var filepath = root + @"\" +
                            site + @"\" +
                            @"nct\" +
                            dtPatientId.ToString(@"yyyy") + @"\" +
                            dtPatientId.ToString(@"MM") + @"\" +
                            dtPatientId.ToString(@"yyyyMMddHHmmss") + @".jpg";


            FileInfo fInfo = new FileInfo(filepath);

            if (!fInfo.Exists)
            {
                error = $"NotFound|Image {fInfo.Name} not found."
                        + (detailsOnError ? Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            ">>ImageController 0002<<" + Environment.NewLine +
                                            "------------------------" + Environment.NewLine +
                                            "filepath: [" + filepath + "]" + Environment.NewLine +
                                            "root: [" + root + "]" + Environment.NewLine +
                                            "site: [" + site + "]" + Environment.NewLine +
                                            "patientId: [" + patientId + "]" + Environment.NewLine 
                                          : "");
                return null;
            }

            if (!new FileExtensionContentTypeProvider().TryGetContentType(fInfo.FullName, out string contentType))
            {
                throw new ArgumentOutOfRangeException($"Unable to find Content Type for file name {fInfo.Name}.");
            }

            return PhysicalFile(filepath, contentType);
        }

        private PhysicalFileResult GetPatientIndicatorImageFile(string number, GetPatientBy getPatientBy, string imageName, out string error, bool detailsOnError = false)
        {
            error = "";

            Dictionary<string, string> externalRootSitePatientId = _patientService.GetExternalRootSitePatientId(number, getPatientBy, OptionNames.CUSTOM_INDICATORS_IMAGE_PATH.ToString());

            if (externalRootSitePatientId == null)
            {
                error = "BadRequest|";
                return null;
            }

            var root = externalRootSitePatientId["root"];

            if (root == null)
            {
                error = "BadRequest|"
                        + (detailsOnError ? Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            ">>ImageController 0003<<" + Environment.NewLine +
                                            "------------------------" + Environment.NewLine +
                                            "root: [" + root + "]" + Environment.NewLine
                                          : "");
                return null;
            }

            //var filepath = @"\\ros-57c-dx01.picis.com\E$\git\pulsecheck\root\images\custom_indicators\Cigarette.png";
            var filepath = root + @"\" +
                            imageName;

            FileInfo fInfo = new FileInfo(filepath);

            if (!fInfo.Exists)
            {
                error = $"NotFound|Image {fInfo.Name} not found."
                        + (detailsOnError ? Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            Environment.NewLine +
                                            ">>ImageController 0004<<" + Environment.NewLine +
                                            "------------------------" + Environment.NewLine +
                                            "filepath: [" + filepath + "]" + Environment.NewLine +
                                            "root: [" + root + "]" + Environment.NewLine 
                                          : "");
                return null;
            }

            if (!new FileExtensionContentTypeProvider().TryGetContentType(fInfo.FullName, out string contentType))
            {
                throw new ArgumentOutOfRangeException($"Unable to find Content Type for file name {fInfo.Name}.");
            }

            return PhysicalFile(filepath, contentType);
        }
    }
}
using System;
using System.Linq;
using Emar.Api.Helpers;
using Emar.Core.Helpers;
using Emar.Core.Users.Model;
using Emar.Core.Users.Repository;
using Emar.Core.Users.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public UsersController(IUserService userService,
                                IPropertyCheckerService propertyCheckerService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = nameof(GetUsers))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserDto> GetUsers(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery(Name = "fields")] string fields,
            [FromQuery(Name = "extId")] string extId
            )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (!_propertyCheckerService.TypeHasProperties<UserDto>(fields))
            {
                return BadRequest();
            }

            if (extId != null)
            {
                //int xId = (int)_userRepository.GetInternalUserId(extId);
                var user = _userService.GetUserByExternalId(extId);

                if (user == null)
                {
                    return NotFound($"User with external id '{extId}' was not found.");
                }

                return Ok(user);
            }

            return null;
        }

        [HttpGet("{userId}", Name = nameof(GetUser))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserDto> GetUser(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromRoute(Name = "userId")] int userId,
            [FromQuery(Name = "fields")] string fields
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
            {
                return BadRequest("Unsupported media type header provided.");
            }

            if (!_propertyCheckerService.TypeHasProperties<UserDto>(fields))
            {
                return BadRequest();
            }

            var user = _userService.GetUser(userId);

            if (user == null)
            {
                return NotFound($"User with id '{userId}' was not found.");
            }

            return Ok(user.ShapeData(fields));
        }

        /// <summary>
        /// Retrieve a list of physicians who can be used to fill in the "Ordering Physician" field in the Cart Check-out modal
        /// </summary>
        /// <param name="mediaType">Acceptable Media Types</param>
        /// <param name="siteId">Site to retrieve the Physicians for</param>
        /// <param name="patientId">Patient we're retrieving the list for - optional, but if provided, the patient's ER Attending Doc will be provided for a default</param>
        /// <returns></returns>
        [HttpGet("orderingphysicians", Name = nameof(GetOrderingPhysicianList))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserDto> GetOrderingPhysicianList(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromHeader(Name = "EMAR-Site")] int? siteId,
            [FromHeader(Name = "EMAR-Patient")] long? patientId
        )
        {
            if (!MediaTypes.IsValidMediaType(mediaType))
                return BadRequest("Unsupported media type header provided.");

            if (siteId == null)
            {
                return BadRequest("Site id is missing.");
            }

            var physicianList = _userService.GetOrderingPhysicians(siteId.Value, patientId ?? 0);

            if (physicianList == null)
                return NotFound($"Site {siteId} doesn't have any Active ordering Physicians to choose from.");

            return Ok(physicianList);
        }
    }
}
using System;
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
        private IUserService _userService;
        private IUserRepository _userRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public UsersController(IUserService userService,
                                IUserRepository userRepository,
                                IPropertyMappingService propertyMappingService,
                                IPropertyCheckerService propertyCheckerService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = nameof(GetUsers))]
        [ProducesResponseType(200)] // (OK) - the resource is sent in the response
        [ProducesResponseType(400)] // (bad request) - indicates a bad request (e.g. wrong parameter)
        [ProducesResponseType(404)] // (not found) - the resource does not exits
        [ProducesResponseType(406)] // (not acceptable) - the server does not support the required representation
        public ActionResult<UserDto> GetUsers(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string fields,
            [FromQuery] string extId
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
                int xId = (int)_userRepository.GetInternalUserId(extId);

                if (xId == 0)
                {
                    return NotFound($"User with external id '{extId}' was not found.");
                }

                return GetUser(mediaType, fields, xId);
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
            [FromQuery] string fields,
            int userId
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
    }
}
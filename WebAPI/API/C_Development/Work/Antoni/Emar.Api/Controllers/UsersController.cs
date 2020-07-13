using System;
using Emar.Core;
using Emar.Core.Users.Model;
using Emar.Core.Users.Service;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public UsersController(IUserService userService,
                                  IPropertyMappingService propertyMappingService,
                                  IPropertyCheckerService propertyCheckerService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet("{userId}", Name = nameof(GetUser))]
        public ActionResult<UserDto> GetUser(
            [FromHeader(Name = "Accept")] string mediaType,
            [FromQuery] string fields,
            int userId
            )
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<UserDto>(fields))
            {
                return BadRequest();
            }

            var user = _userService.GetUser(userId);

            if (user == null) { return NotFound($"User with id {userId} was not found"); }

            if (String.IsNullOrEmpty(fields))
            {
                fields =
                    nameof(user.Id) + "," +
                    nameof(user.Name) + "," +
                    nameof(user.SiteId) + "," +
                    nameof(user.SiteName);
            }

            return Ok(user.ShapeData(fields));
        }
    }
}
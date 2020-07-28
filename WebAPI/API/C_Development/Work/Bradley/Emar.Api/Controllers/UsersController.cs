using System;
using Emar.Api.Helpers;
using Emar.Core;
using Emar.Core.Helpers;
using Emar.Core.Orders.Service;
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
        private IOrderService _orderService;

        public UsersController(IUserService userService,
                                IUserRepository userRepository,
                                IPropertyMappingService propertyMappingService,
                                IPropertyCheckerService propertyCheckerService,
                                IOrderService orderService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
            _orderService = orderService;
        }

        [HttpGet(Name = nameof(GetUsers))]
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

            var user = _userService.GetUserMinimal(userId);

            if (user == null) { return NotFound($"User with id '{userId}' was not found."); }

            //if (String.IsNullOrEmpty(fields))
            //{
            //    fields =
            //        nameof(user.Id) + "," +
            //        nameof(user.DisplayName) + "," +
            //        (!user.NameDisplayInitials ? nameof(user.FirstName) + "," +
            //                                     nameof(user.MiddleName) + "," +
            //                                     nameof(user.LastName) + "," +
            //                                     nameof(user.NameSuffix) + ","
            //                                   : "") +
            //        nameof(user.SiteId) + "," +
            //        nameof(user.Site) + "." + nameof(user.Site.Name);
            //}

            return Ok(user.ShapeData(fields));
        }
    }
}
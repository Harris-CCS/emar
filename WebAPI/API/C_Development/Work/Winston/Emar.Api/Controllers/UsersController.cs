using System;
using Emar.Core.Users.Service;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    //[Produces(MediaTypes.PcEmar, MediaTypes.Json)]
    [Consumes(MediaTypes.PcEmar, MediaTypes.Json)]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        }
        [HttpGet("{userId}", Name = nameof(Getuser))]
        public IActionResult Getuser(int userId)
        {
            var user = _userService.GetUser(userId);

            return Ok(user);
        }

    }
}
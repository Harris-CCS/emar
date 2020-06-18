using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emar.Core.Users.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ??
                              throw new ArgumentNullException(nameof(userService));

        }
        [HttpGet("{userId}")]
        public IActionResult Getuser(int userId)
        {
            var user = _userService.GetUser(userId);
            return new JsonResult(user);
        }

    }
}
using System;
using Emar.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Emar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypes.Json)]
    [Consumes(MediaTypes.Json)]
    public class ErrorsController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            // Get the details of the exception that occurred
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                // Get which route the exception occurred at
                string routeWhereExceptionOccurred = exceptionFeature.Path;

                // Get the exception that occurred
                Exception exceptionThatOccurred = exceptionFeature.Error;

                // TODO: Do something with the exception
                // Log it with Serilog?
                // Send an e-mail, text, fax, or carrier pidgeon?  Maybe all of the above?
                // Whatever you do, be careful to catch any exceptions, otherwise you'll end up with a blank page and throwing a 500
            }

            /////return StatusCode(StatusCodes.Status500InternalServerError);
            return BadRequest();
        }
    }
}

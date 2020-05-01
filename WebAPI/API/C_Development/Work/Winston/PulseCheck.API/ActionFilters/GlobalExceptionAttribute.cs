using System;
using System.Web.Http.Filters;
using System.Web.Http;
using System.Web.Http.Tracing;
using PulseCheck.API.Helpers;
using PulseCheck.API.Models;
using System.Net.Http;
using System.Net;

namespace PulseCheck.API.ActionFilters
{
    /// <summary>
    /// Action filter to handle for Global application errors.
    /// </summary>
    public class GlobalExceptionAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Handle exceptions
        /// </summary>
        /// <param name="context">HttpActionExecutedContext instance</param>
        public override void OnException(HttpActionExecutedContext context)
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NLogger());
            var trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            trace.Error(context.Request, "Controller : " + context.ActionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName + Environment.NewLine + "Action : " + context.ActionContext.ActionDescriptor.ActionName, context.Exception);

            var exceptionType = context.Exception.GetType();

            if (exceptionType == typeof(UnauthorizedAccessException))
            {
                throw new WLRResponse(ErrorCodes.NOT_AUTHORIZED, context.Request);
            }
            else if (exceptionType == typeof(Utilities.Exceptions.NonFatalException))
            {
                // TODO: do we need to do something here?  
            }
            else
            {
                var ex = new WLRResponse(ErrorCodes.UNEXPECTED_ERROR_CONDITION, context.Request);
                throw new HttpResponseException(context.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.AssembleResponse()));
            }

        }
    }
}
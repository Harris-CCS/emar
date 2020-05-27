using System;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Web.Http.Tracing;
using System.Web.Http;
using PulseCheck.API.Helpers;
using PulseCheck.API.Actions;
using System.Linq;

namespace PulseCheck.API.ActionFilters
{
    /// <summary>
    /// Log action execution in NLog
    /// </summary>
    public class LoggingFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// OnActionExecuting logging handler
        /// </summary>
        /// <param name="filterContext">HttpActionContext instance</param>
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (filterContext.ActionDescriptor.GetCustomAttributes<DisableRequestLogging>().Any())
            {
                return;
            }

            GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NLogger());
            var trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            trace.Info(filterContext.Request, "Controller : " + filterContext.ControllerContext.ControllerDescriptor.ControllerType.FullName + Environment.NewLine + "Action : " + filterContext.ActionDescriptor.ActionName, "JSON", filterContext.ActionArguments);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;

namespace PulseCheck.Utilities
{
    public class Log
    {
        public static void Error(Exception ex, string customErrorMessage = null)
        {
            //GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new NLogger());
            //var trace = GlobalConfiguration.Configuration.Services.GetTraceWriter();
            //trace.Error(context.Request, "Controller : " + context.ActionContext.ControllerContext.ControllerDescriptor.ControllerType.FullName + Environment.NewLine + "Action : " + context.ActionContext.ActionDescriptor.ActionName, context.Exception);
             
        }
    }
}

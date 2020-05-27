using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NLog;

namespace PulseCheck.API.Controllers
{
    public abstract class ControllerBase : ApiController
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ControllerBase()
        {
        }
    }
}

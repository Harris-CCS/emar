using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using System.Web;

namespace PulseCheck.API
{
    /// <summary>
    /// Web API global
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// API startup configuration
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RazorEngineConfig.CompileTemplates();
        }

        /// <summary>
        /// Handle requests to the API
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                // When doing a pre-flight for an AJAX call, just return, since the headers will handle it
                HttpContext.Current.Response.End();
            }
        }
    }

    /// <summary>
    /// Class containing information about error codes
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Code for when we determine that a provided authorization is invalid
        /// </summary>
        public const char INVALID_AUTHORIZATION = 'A';

        /// <summary>
        /// Code for user authentication issues
        /// </summary>
        public const char USER_AUTHENTICATION = 'B';

        /// <summary>
        /// Code for when we find that a user does not have a cookie.
        /// </summary>
        public const char NO_COOKIE_FOUND = 'C';

        /// <summary>
        /// Code for when we cannot connect to the database
        /// </summary>
        public const char DATA_SERVER_FAILED = 'D';

        /// <summary>
        /// Catch-all code for when something goes wrong, typically errors that would result in a 500 status code
        /// </summary>
        public const char UNEXPECTED_ERROR_CONDITION = 'E';

        /// <summary>
        /// Code for when something goes wrong while performing operations on the database
        /// </summary>
        public const char DATA_WRITE_FAILED = 'F';

        /// <summary>
        /// Code for when a user is not authorized to perform the action which they are trying to perform
        /// </summary>
        public const char NOT_AUTHORIZED = 'G';

        /// <summary>
        /// Code for when a session type is not valid
        /// </summary>
        public const char INVALID_SESSION_TYPE = 'H';

        /// <summary>
        /// Code for when we determine that user credentials are actively being used from multiple devices
        /// </summary>
        public const char SOMEONE_ELSE_LOGGED_IN = 'I';

        /// <summary>
        /// Code for when an authorization has expired
        /// </summary>
        public const char EXPIRED_AUTHORIZATION = 'J';

        /// <summary>
        /// Code for when a provided input does not pass validation
        /// </summary>
        public const char PARAMETER_FAULT = 'K';

        /// <summary>
        /// Code for when an authorization is corrupted
        /// </summary>
        public const char CORRUPTED_AUTHORIZATION = 'L';

        /// <summary>
        /// Code for when we cannot find a session for the user
        /// </summary>
        public const char NO_SESSION = 'N';

        /// <summary>
        /// Code for errors encountered in IP authorization
        /// </summary>
        public const char IP_AUTH_ERROR = 'P';

        /// <summary>
        /// Code for an issue that would require the user to log in again
        /// </summary>
        public const char RE_LOGIN = 'R';

        /// <summary>
        /// Code for when the session times out
        /// </summary>
        public const char TIMEOUT = 'T';

        /// <summary>
        /// Code for when we determine that the user is not valid
        /// </summary>
        public const char INVALID_USER = 'U';

        #region Animals
        /// <summary>
        /// Image number for the bunny error
        /// </summary>
        private const string BUNNY = "3";

        /// <summary>
        /// Image number for the butterfly error
        /// </summary>
        private const string BUTTERFLY  =  "6";

        /// <summary>
        /// Image number for the dolphin error
        /// </summary>
        private const string DOLPHIN    =  "9";

        /// <summary>
        /// Image number for the elephant error
        /// </summary>
        private const string ELEPHANT   =  "8";

        /// <summary>
        /// Image number for the giraffe error
        /// </summary>
        private const string GIRAFFE    =  "7";

        /// <summary>
        /// Image number for the red penguin error
        /// </summary>
        private const string RED_PENGUIN =  "5";

        /// <summary>
        /// Image number for the penguin error
        /// </summary>
        private const string PENGUIN    =  "4";

        /// <summary>
        /// Image number for the polar bear error
        /// </summary>
        private const string POLAR_BEAR = "10";

        /// <summary>
        /// Image number for the turtle error
        /// </summary>
        private const string TURTLE     =  "1";

        /// <summary>
        /// Image number for the tiger error
        /// </summary>
        private const string TIGER      =  "2";
        #endregion

        /// <summary>
        /// Dictionary of information that defines the message and status code to return for each error code.
        /// </summary>
        public static Dictionary<char, AnimalError> errors = new Dictionary<char, AnimalError>()
        {
            { INVALID_AUTHORIZATION,      new AnimalError { message = "Invalid authorization", animal = ELEPHANT } },
            { USER_AUTHENTICATION,        new AnimalError { message = "User Authentication", animal = GIRAFFE } },
            { NO_COOKIE_FOUND,            new AnimalError { message = "No Cookie Found", animal = ELEPHANT } },
            { DATA_SERVER_FAILED,         new AnimalError { message = "Data server failed", animal = TURTLE, statusCode = 500 } },
            { UNEXPECTED_ERROR_CONDITION, new AnimalError { message = "An unexpected error condition occurred that stopped program execution", animal = POLAR_BEAR, statusCode = 500 } },
            { DATA_WRITE_FAILED,          new AnimalError { message = "Data write failed", animal = TIGER, statusCode = 500 } },
            { NOT_AUTHORIZED,             new AnimalError { message = "Not authorized", animal = BUNNY, additionalInfo = "You do not have permission to perform the task which you tried to perform. Under normal circumstances you should not be getting this error. Please contact Harris support at your earliest convenience to report this problem.", statusCode = 403} },
            { INVALID_SESSION_TYPE,       new AnimalError { message = "Invalid session type", animal = ELEPHANT } },
            { SOMEONE_ELSE_LOGGED_IN,     new AnimalError { message = "Someone else logged in using your identity.", animal = PENGUIN } },
            { EXPIRED_AUTHORIZATION,      new AnimalError { message = "Expired authorization", animal = ELEPHANT } },
            { PARAMETER_FAULT,            new AnimalError { message = "Parameter fault", animal = RED_PENGUIN, statusCode = 400 } },
            { CORRUPTED_AUTHORIZATION,    new AnimalError { message = "Corrupted authorization", animal = ELEPHANT } },
            { NO_SESSION,                 new AnimalError { message = "No session", animal = ELEPHANT } },
            { IP_AUTH_ERROR,              new AnimalError { message = "IP Authorization Error", animal = ELEPHANT } },
            { RE_LOGIN,                   new AnimalError { message = "Re-login", animal = null } },
            { TIMEOUT,                    new AnimalError { message = "Time out", animal = BUTTERFLY } },
            { INVALID_USER,               new AnimalError { message = "Invalid user exception", animal = DOLPHIN, statusCode = 500} }
        };
    }

    /// <summary>
    /// Animal error representation for the API
    /// </summary>
    public class AnimalError
    {
        /// <summary>
        /// Short message to display for animal error
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Animal image identifier
        /// </summary>
        public string animal { get; set; }

        /// <summary>
        /// Additional information about the error
        /// </summary>
        public string additionalInfo { get; set; } = null;

        /// <summary>
        /// HTTP Status code associated with the error
        /// </summary>
        public int statusCode { get; set; } = 401; 
    }
}

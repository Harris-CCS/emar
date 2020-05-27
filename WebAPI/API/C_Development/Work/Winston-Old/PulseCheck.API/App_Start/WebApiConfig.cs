using PulseCheck.API.ActionFilters;
using PulseCheck.API.Helpers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace PulseCheck.API
{
    /// <summary>
    /// Web API configuration
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register configuration
        /// </summary>
        /// <param name="config">HttpConfiguration instance</param>
        public static void Register(HttpConfiguration config)
        {
            //// Web API configuration and services
            //config.EnableCors();

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Default Web API route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Request logging
            config.Filters.Add(new LoggingFilterAttribute());

            // Exception logging
            config.Filters.Add(new GlobalExceptionAttribute());
        }
    }
}

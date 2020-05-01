using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace PulseCheck.API.App_Start
{
    /// <summary>
    /// Class to add Authorization header to Swagger UI documentation where needed
    /// </summary>
    public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Apply the header change
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="schemaRegistry">Schema Registry</param>
        /// <param name="apiDescription">API description</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            // Add the Authorization header input field when the route does not allow anonymous requests.
            var allowAnonymous = apiDescription.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
            if (!allowAnonymous)
            {
                if (operation.parameters == null)
                {
                    operation.parameters = new List<Parameter>();
                }

                operation.parameters.Add(new Parameter
                {
                    name = "Authorization",
                    @in = "header",
                    type = "string",
                    required = true
                });
            }
        }
    }
}
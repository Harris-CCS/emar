using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace PulseCheck.API.Policies
{
    /// <summary>
    /// Policy for allowing CORS requests.  Allowed servers should be set in web.config.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class APICorsPolicy : Attribute, ICorsPolicyProvider
    {
        private CorsPolicy _policy;

        /// <summary>
        /// Custom CORS policy for the API
        /// </summary>
        public APICorsPolicy()
        {
            _policy = new CorsPolicy
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true,
                SupportsCredentials = true,
            };

            _policy.Origins.Add("*");

            // Add allowed origins.
            //var origins = ConfigurationManager.AppSettings["CORSOrigin"].Split(',');
            //foreach (var origin in origins)
            //{
            //    _policy.Origins.Add(origin);
            //}
        }

        /// <summary>
        /// Get the custom policy
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The CORS policy</returns>
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request)
        {
            return Task.FromResult(_policy);
        }

        /// <summary>
        /// Get the custom policy
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The CORS policy</returns>
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_policy);
        }
    }
}
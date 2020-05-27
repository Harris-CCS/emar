using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Owin;
using IdentityModel.Client;
using Newtonsoft.Json;
using Host.Configuration;
using PulseCheck.IOC.Mappings;
using PulseCheck.QCPR.Domain.Contract;
using PulseCheck.QCPR.Logic.Bindings;

[assembly: OwinStartup(typeof(PulseCheck.API.Startup))]

namespace PulseCheck.API
{    
    /// <summary>
    /// PulseCheck API startup information
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// PulseCheck API startup configuration
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = Addresses.GetIDServerBaseAddress(),
                ValidationMode = ValidationMode.ValidationEndpoint,
                RequiredScopes = new[]
                {
                    Constants.Identifiers.APIScopeName,
                    Constants.Identifiers.PasswordChangeScopeName,
                },
                PreserveAccessToken = true,
                EnableValidationResultCache = true,
                ValidationResultCacheDuration = new TimeSpan(0,0,1,0)
            });

            // claims transformation
            app.UseClaimsTransformation(new ClaimsTransformer().Transform);

            // configure web api
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Require authentication for all controllers
            config.Filters.Add(new System.Web.Http.AuthorizeAttribute());

            // Log all requests
            config.Filters.Add(new PulseCheck.API.ActionFilters.LoggingFilterAttribute());

            // Handle exceptions
            config.Filters.Add(new PulseCheck.API.ActionFilters.GlobalExceptionAttribute());

            // Register your Web API controllers.            
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // OPTIONAL: Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);

            builder.RegisterAssemblyModules(
                new[]
                {
                    typeof(RegisterApplicationIocAutoFac).Assembly,
                    typeof(AutoFacQcprLogicRegistrations).Assembly
                });

            var container = builder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            
            // Register the Autofac middleware FIRST, then the Autofac Web API middleware,
            // and finally the standard Web API middleware.
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);

            var qcprManager = AutoFacQcprLogicRegistrations.GetType<IQcprManager>(container);
            qcprManager?.ReloadCachedImportDataFromTable();
        }
    }

    /// <summary>
    /// Claims transformer for PulseCheck API
    /// </summary>
    public class ClaimsTransformer
    {
        /// <summary>
        /// Given a ClaimsPrincipal, perform a claims transformation
        /// </summary>
        /// <param name="incomingPrincipal"></param>
        /// <returns></returns>
        public async Task<ClaimsPrincipal> Transform(ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
            {
                return await Task.FromResult(incomingPrincipal);
            }

            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return await Task.FromResult(incomingPrincipal);
            }

            var header = authHeader.Split(' ');

            if (header.Length != 2)
            {
                return await Task.FromResult(incomingPrincipal);
            }

            var accessToken = header[1];
            var claimsIdentity = await GetClaimsIdentity(accessToken);

            var claimsToAdd = claimsIdentity.Claims.Where(p => !incomingPrincipal.Identities.First().Claims.Any(p2 => p2.Type == p.Type && p2.Value == p.Value));
            incomingPrincipal.Identities.First().AddClaims(claimsToAdd);

            return await Task.FromResult(incomingPrincipal);
        }

        static async Task<ClaimsIdentity> GetClaimsIdentity(string token)
        {
            var client = new UserInfoClient(
                new Uri(string.Format(Constants.Endpoints.UserInfoEndpoint, Addresses.GetIDServerBaseAddress())),
                token);

            var response = await client.GetAsync();
            return response.GetClaimsIdentity();
        }
    }
}
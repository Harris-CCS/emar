using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Dependency.Resolver;
using Host.Configuration;
using Owin;
using Microsoft.Owin;
using Serilog;
using IdentityManager.Configuration;
using IdentityManager.Core.Logging;
using IdentityManager.Logging;
using Host.Web.Mvc.IdMgr;
using IdentityServer3.Core.Configuration;
using Microsoft.Owin.Security.Cookies;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;

namespace Host.Web.Mvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            var config = new MembershipRebootConfiguration();
            config.PasswordHashingIterationCount = 10000;

            var appinfo = new OwinApplicationInformation(
                app, 
                "Test",
                "Test Email Signature",
                "UserAccount/Login",
                "UserAccount/ChangeEmail/Confirm/",
                "UserAccount/Register/Cancel/",
                "UserAccount/PasswordReset/Confirm/");

            var emailFormatter = new EmailMessageFormatter(appinfo);
            // uncomment if you want email notifications -- also update smtp settings in web.config
            config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

            LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Trace()
               .CreateLogger();

            var connectionString = "PulseCheck.Membership";

            app.Map("/admin", adminApp =>
            {
                var factory = new IdentityManagerServiceFactory();
                factory.Configure(connectionString);
                
                adminApp.UseIdentityManager(new IdentityManagerOptions()
                {       
                    Factory = factory,
                    //SecurityConfiguration = new HostSecurityConfiguration()
                    //{
                    //    AdminRoleName = "admin",
                    //    HostAuthenticationType = "Cookies",
                    //    RoleClaimType = "role",
                    //}
                });
            });

            var options = new IdentityServerOptions()
            {
                SiteName = "PulseCheck - Custom Login",                
                AuthenticationOptions = new IdentityServer3.Core.Configuration.AuthenticationOptions()
                {

                },
                EventsOptions = new EventsOptions()
                {
                    RaiseSuccessEvents = true,
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true
                }
            };

            builder.RegisterAssemblyModules(typeof(RegisterApplicationIocAutoFac).Assembly);

            // Register your MVC controllers. (MvcApplication is the name of
            // the class in Global.asax.)
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            builder.RegisterInstance(config);

            app.UseAutofacMiddleware(container);
            app.UseIdentityServer(connectionString, options);
        }
    }
}
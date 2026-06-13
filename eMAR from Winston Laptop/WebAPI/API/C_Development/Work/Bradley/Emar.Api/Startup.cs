using Emar.Core.Carts.Repository;
using Emar.Core.Carts.Service;
using Emar.Core.Devices.Repository;
using Emar.Core.Devices.Service;
using Emar.Core.Helpers;
using Emar.Core.HomeMedications.Repository;
using Emar.Core.HomeMedications.Service;
using Emar.Core.InboundData.Repository;
using Emar.Core.InboundData.Service;
using Emar.Core.InboundData.Service.IbexSpecific;
using Emar.Core.Medications.Repository;
using Emar.Core.Medications.Service;
using Emar.Core.Notifications.Repository;
using Emar.Core.Notifications.Service;
using Emar.Core.Options.Repository;
using Emar.Core.Options.Service;
using Emar.Core.Orders.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Patients.Repository;
using Emar.Core.Patients.Service;
using Emar.Core.Sites.Repository;
using Emar.Core.Sites.Service;
using Emar.Core.Templates.Repository;
using Emar.Core.Templates.Service;
using Emar.Core.Users.Repository;
using Emar.Core.Users.Service;
using Emar.Core.WinstonTests.Repository;
using Emar.Core.WinstonTests.Service;
using Emar.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Emar.Core.OutboundData.Repository;
using Emar.Core.OutboundData.Service;
using Emar.Core.OutboundData.Service.IbexSpecific;
using Emar.Core.OutboundChart.Repository;
using Emar.Core.OutboundChart.Service;
using Emar.Core.PharmacyNotifications;

namespace Emar.Api
{
    public class Startup
    {
        private string EmarOpenAPISpecification = "eMAROpenAPISpecification";
        private string EmarOpenAPITitle = "eMAR API";

        #region Constructors
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpCacheHeaders((expirationModelOptions) =>
            {
                expirationModelOptions.MaxAge = 60;
                expirationModelOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            },
            (validationModelOptions) =>
            {
                validationModelOptions.MustRevalidate = true;
            });

            services.AddResponseCaching();

            services.AddControllers(setupAction =>
            {
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));

                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.RespectBrowserAcceptHeader = true;
                setupAction.CacheProfiles.Add("240SecondsCacheProfile",
                    new CacheProfile()
                    {
                        Duration = 240
                    });

                //Disable client-side caching of the response from any API endpoints.
                //This sets the value of the "Pragma" response header to "no-cache" which
                //should result in the client browser not caching results.
                //Winston Murdock, 04/04/2021.
                setupAction.CacheProfiles.Add("DisableCaching",
                    new CacheProfile()
                    {
                        Duration = 0,
                        NoStore = true,
                        Location = ResponseCacheLocation.None
                    });
            })
                .AddNewtonsoftJson(setupAction =>
                {
                    setupAction.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver();
                })
                //.AddXmlDataContractSerializerFormatters()
                .ConfigureApiBehaviorOptions(setupAction =>
                {
                    setupAction.InvalidModelStateResponseFactory = context =>
                    {
                        // create a problem details object
                        var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);

                        // add additional info not added by default
                        problemDetails.Detail = "See the errors field for details.";
                        problemDetails.Instance = context.HttpContext.Request.Path;

                        // find out which status code to use
                        var actionExecutingContext = context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                        // if there are modelstate errors & all arguments were correctly
                        // found/parsed we're dealing with validation errors
                        if ((context.ModelState.ErrorCount > 0) &&
                            (actionExecutingContext?.ActionArguments.Count == context.ActionDescriptor.Parameters.Count))
                        {
                            problemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                            problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                            problemDetails.Title = "One or more validation errors occurred.";

                            return new UnprocessableEntityObjectResult(problemDetails)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        }

                        // if one of the arguments wasn't correctly found / couldn't be parsed
                        // we're dealing with null/unparseable input
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Title = "One or more errors on input occurred.";
                        return new BadRequestObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                if (newtonsoftJsonOutputFormatter != null)
                {
                    ////newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add(Controllers.MediaTypes.PcEmar);

                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (newtonsoftJsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                    {
                        newtonsoftJsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                    }
                }
            });

            //*****************************************************************************
            //If we are debugging in the IDE (F5), then enable sensitive data logging.
            //If we are running inside IIS or running in the IDE but not debugging (Ctrl-F5),
            //then do not enable sensitive data logging.
            //This prevents any PHI from showing up in the Event Viewer (or command prompt).
            //Credit to Brad for this link and the suggestion for this.
            //https://www.fmsinc.com/free/NewTips/NET/NETtip32.asp
            //EMAR-625
            //Winston Murdock, 01/25/2021.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //We are debugging in the IDE (F5).
                //Enable sensitive logging.
                services.AddDbContext<EmarContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("SqlConnection"))
                        .EnableSensitiveDataLogging());

                services.AddDbContext<IbexContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("IbexSqlConnection"))
                        .EnableSensitiveDataLogging());
            }
            else
            {
                //We were getting "An exception has been raised that is likely
                //due to a transient failure. Consider enabling transient error
                //resiliency by adding 'EnableRetryOnFailure()' to the
                //'UseSqlServer' call" errors.
                //The rcommended solution is to add EnableRetryOnFailure when
                //setting up the DB context.
                //
                //Colin decided on five retries with a one second interval between them since
                //any call that is deadlocked could be something that the UI is waiting on.
                //If we change our mind on the number of retires or the interval, we change
                //the maxRetryCount and maxRetryDelay values below for both DB contexts.
                //
                //We don't need to bother with this when debugging within Visual Studio.
                //So I'm not enabling retry on failure in the if block above.

                //https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-resilient-entity-framework-core-sql-connections
                //Winston Murdock, 12/10/2021.  PC-26692

                //Commented out the calls to EnableRetryOnFailure because it gave us issues on 57c.
                //I need to do more research into this.

                //We are running inside IIS or running in the IDE but not debugging (Ctrl-F5).
                //Don't enable sensitive logging.
                services.AddDbContext<EmarContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")
                    //,
                    //sqlServerOptionsAction: sqlOptions =>
                    //{
                    //    sqlOptions.EnableRetryOnFailure
                    //    (
                    //        maxRetryCount: 5,
                    //        maxRetryDelay: TimeSpan.FromSeconds(1),
                    //        errorNumbersToAdd: null
                    //    );
                    //}
                    ));

                services.AddDbContext<IbexContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("IbexSqlConnection")
                    //,
                    //sqlServerOptionsAction: sqlOptions =>
                    //{
                    //    sqlOptions.EnableRetryOnFailure
                    //    (
                    //        maxRetryCount: 5,
                    //        maxRetryDelay: TimeSpan.FromSeconds(1),
                    //        errorNumbersToAdd: null
                    //    );
                    //}
                    ));
            }
            //*****************************************************************************

            services.AddSingleton<EmarMemoryCache>();

            services.AddScoped<ICartOrderService, CartOrderService>();
            services.AddScoped<ICartOrderRepository, CartOrderRepository>();

            //////services.AddScoped<ICodeShareRepository, CodeShareRepository>();

            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();

            services.AddScoped<IDoseRangeCheckingInfoService, DoseRangeCheckingInfoService>();
            services.AddScoped<IDoseRangeCheckingInfoRepository, DoseRangeCheckingInfoRepository>();

            services.AddScoped<IHomeMedicationService, HomeMedicationService>();
            services.AddScoped<IHomeMedicationRepository, HomeMedicationRepository>();

            services.AddScoped<IInteractionRepository, InteractionRepository>();

            services.AddScoped<IMedicationService, MedicationService>();
            services.AddScoped<IMedicationRepository, MedicationRepository>();

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<IOptionService, OptionService>();
            services.AddScoped<IOptionRepository, OptionRepository>();

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IPatientRepository, PatientRepository>();

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<ISiteRepository, SiteRepository>();

            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IWinstonTestService, WinstonTestService>();
            services.AddScoped<IWinstonTestRepository, WinstonTestRepository>();

            services.AddScoped<IOdsEmarOutboundService, OdsEmarOutboundService>();
            services.AddScoped<IEmarOutboundDataRepository, EmarOutboundDataRepository>();

            services.AddScoped<IOcsEmarOutboundService, OcsEmarOutboundService>();
            services.AddScoped<IEmarOutboundChartRepository, EmarOutboundChartRepository>();

            if (Configuration.GetValue<bool>("PharmacyNotificationsServiceEnabled"))
            {
                services.AddHostedService<PharmacyNotificationService>();
            }

            #region IDS Services

            if (!Configuration.GetValue<bool>("BypassIds"))
            {
                // Background Hosted Services - explained in:
                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio

                // Scoped services specific to IDS
                services.AddScoped<IIbexIdsProcessorService, IbexIdsProcessorService>();
                services.AddScoped<IIbexInboundDataRepository, IbexInboundDataRepository>();
                services.AddScoped<IIdsEmarUpdateService, IdsEmarUpdateService>();
                
                // Setup the Channel to communicate between the SQL listener service and the processor service
                services.AddSingleton<SqlQueueNotificationChannel>();

                // Setup the processor hosted service first so it can be ready to pull stuff off the channel before we put anything in
                // (probably not crucial, but won't hurt, and makes logical sense) 
                services.AddHostedService<IbexSqlMessageProcessorHostedService>();

                // Setup the SQL Listening hosted service last - to start pulling SQL notices from the SQL service
                // and shove them into the Channel
                services.AddHostedService<IbexSqlListenerHostedService>();
            }

            #endregion


            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(EmarOpenAPISpecification, new OpenApiInfo()
                {
                    Title = EmarOpenAPITitle,
                    Version = "1",
                    Description = "API for eMAR (Electronic Medicine Administration Record)"//,
                    //Contact = new OpenApiContact()
                    //{
                    //    Email = "",
                    //    Name = "",
                    //    Url = new Uri(""),
                    //},
                    //License = new OpenApiLicense()
                    //{
                    //    Name = "",
                    //    Url = new Uri("")
                    //}
                });

                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                setupAction.IncludeXmlComments(xmlCommentsFullPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        //IHostApplicationLifetime appLifetime)
        {
            //// IHostApplicationLifetime allows us to fire OnStarted, OnStopping and OnStopped events ////
            //appLifetime.ApplicationStarted.Register(OnStarted);
            //appLifetime.ApplicationStopping.Register(OnStopping);
            //appLifetime.ApplicationStopped.Register(OnStopped);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });

            }

            app.UseHttpContext();

            // app.UseResponseCaching();

            app.UseHttpCacheHeaders();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(@"/swagger/" + EmarOpenAPISpecification + @"/swagger.json", EmarOpenAPITitle);
                setupAction.RoutePrefix = @"";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

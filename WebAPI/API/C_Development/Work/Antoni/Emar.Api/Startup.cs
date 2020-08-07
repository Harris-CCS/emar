using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Emar.Core.Carts.Repository;
using Emar.Core.Carts.Service;
using Emar.Core.Helpers;
using Emar.Core.Options.Repository;
using Emar.Core.Options.Service;
using Emar.Core.Orders.Repository;
using Emar.Core.Orders.Service;
using Emar.Core.Patients.Repository;
using Emar.Core.Patients.Service;
using Emar.Core.Sites.Service;
using Emar.Core.Sites.Repository;
using Emar.Core.Users.Repository;
using Emar.Core.Users.Service;
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
                var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

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

            services.AddDbContext<EmarContext>(options =>
                options.UseSqlServer(ConfigurationExtensions.GetConnectionString(Configuration, "SqlConnection"))
                    .EnableSensitiveDataLogging());

            services.AddScoped<ICartOrderService, CartOrderService>();
            services.AddScoped<ICartOrderRepository, CartOrderRepository>();

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

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();

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

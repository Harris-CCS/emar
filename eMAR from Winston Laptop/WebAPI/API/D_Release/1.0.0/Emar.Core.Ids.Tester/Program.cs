using Emar.Core.Helpers;
using Emar.Core.InboundData.Repository;
using Emar.Core.InboundData.Service;
using Emar.Core.InboundData.Service.IbexSpecific;
using Emar.Core.Patients.Repository;
using Emar.Core.Sites.Repository;
using Emar.Core.Users.Repository;
using Emar.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Emar.Core.Ids.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            var config = builder.Build();

            //Logger<Program> logger = new Logger<Program>();
            //logger.LogInformation($"Starting the console app");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Standard configs from the API Startup.cs
                    services.AddDbContext<EmarContext>(options =>
                        options.UseSqlServer(config.GetConnectionString("SqlConnection"))
                            .EnableSensitiveDataLogging());

                    services.AddDbContext<IbexContext>(options =>
                        options.UseSqlServer(config.GetConnectionString("IbexSqlConnection"))
                            .EnableSensitiveDataLogging());

                    services.AddSingleton<EmarMemoryCache>();
                    services.AddScoped<IPatientRepository, PatientRepository>(); 
                    services.AddScoped<ISiteRepository, SiteRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IPropertyMappingService, PropertyMappingService>();
                    

                    // Configs specific to the IDS from the API Startup.cs
                    // Background Hosted Services - explained in:
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio
                    services.AddScoped<IIbexIdsProcessorService, IbexIdsProcessorService>();
                    services.AddScoped<IIbexInboundDataRepository, IbexInboundDataRepository>();
                    services.AddScoped<IIdsEmarUpdateService, IdsEmarUpdateService>();

                    services.AddSingleton<SqlQueueNotificationChannel>();

                    services.AddHostedService<IbexSqlMessageProcessorHostedService>();
                    services.AddHostedService<IbexSqlListenerHostedService>();


                    // Testing Service
                    services.AddTransient<IRunTestService, RunTestService>();
                })
                .Build();

            var svc = ActivatorUtilities.CreateInstance<RunTestService>(host.Services);
            svc.CatchUpOnQueueWork();
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
        }
    }
}

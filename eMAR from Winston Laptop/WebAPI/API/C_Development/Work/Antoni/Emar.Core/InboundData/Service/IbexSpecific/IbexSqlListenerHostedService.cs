using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    /// <summary>
    /// Hosted service which will call the scoped service, IEmarIdsSqlTableDependencyManagerService,
    /// which manages the SqlTableDependency connection to the ibex DB, pulling messages from the
    /// SQL Server and passing them into a Channel so that the EmarIdsDataTransfer service can process
    /// the requests sequentially and in a single-threaded fashion
    /// </summary>
    public class IbexSqlListenerHostedService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<IbexSqlListenerHostedService> _logger;

        /// <summary>
        /// Constructor to retrieve the needed DI services needed
        /// </summary>
        /// <param name="services">DI Service pipeline.  Counter pattern, but needed so that we can
        /// call the scoped IEmarIdsSqlTableDependencyManagerService service from within this Hosted (Singleton)
        /// service</param>
        /// <param name="logger">DI to the logging service</param>
        public IbexSqlListenerHostedService(IServiceProvider services, ILogger<IbexSqlListenerHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }
         
        /// <summary>
        /// Worker process which calls the private method which actually does the work.  Only method
        /// required by the BackgroundService
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IbexSqlListenerHostedService Hosted Service running.");

            await DoWork(stoppingToken);
        }
        
        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "IbexSqlListenerService is started.");

            using (var scope = _services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IIbexSqlTableDependencyManagerService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        /// <summary>
        /// override of the base class so that we can log an informational message
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IbexSqlListenerHostedService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}

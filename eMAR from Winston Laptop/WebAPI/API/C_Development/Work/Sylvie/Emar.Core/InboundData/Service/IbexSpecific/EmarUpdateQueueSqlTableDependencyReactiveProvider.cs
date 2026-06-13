using Emar.Data.IbexEntities;
using Microsoft.Extensions.Logging;
using SqlTableDependency.Extensions;
using SqlTableDependency.Extensions.Enums;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Emar.Core.Helpers;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Enums;
using System.Threading.Tasks;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    internal class EmarUpdateQueueSqlTableDependencyReactiveProvider : SqlTableDependencyProvider<EmarUpdateQueue>
    {
        private readonly ILogger<EmarUpdateQueueSqlTableDependencyReactiveProvider> _logger;

        internal EmarUpdateQueueSqlTableDependencyReactiveProvider(ILogger<EmarUpdateQueueSqlTableDependencyReactiveProvider> logger,
            string connectionString, IScheduler scheduler, LifetimeScope lifetimeScope = LifetimeScope.UniqueScope)
            : base(connectionString, scheduler, lifetimeScope)
        {
            _logger = logger;
        }

        // protected override string TableName => base.TableName + "s";

        protected override SqlTableDependencySettings<EmarUpdateQueue> OnCreateSettings()
        {
            var settings = base.OnCreateSettings();

            settings.IncludeOldValues = true;

            return settings;
        }

        protected override ModelToTableMapper<EmarUpdateQueue> OnInitializeMapper(ModelToTableMapper<EmarUpdateQueue> modelToTableMapper)
        {
            modelToTableMapper
                .AddMapping(q => q.Id, "id")
                .AddMapping(q => q.Entity, "entity")
                .AddMapping(q => q.ExternalId, "external_id")
                .AddMapping(q => q.EventDatetime, "event_datetime")
                .AddMapping(q => q.InprocessDatetime, "inprocess_datetime")
                .AddMapping(q => q.CompleteDatetime, "complete_datetime");

            return modelToTableMapper;
        }

        protected override void OnError(Exception e)
        {
            _logger.LogCritical($"SqlTableDependency threw an error:  {Utilities.ExtractExceptionMessages(e)}");
        }

        internal static async Task DoWork(ILogger<EmarUpdateQueueSqlTableDependencyReactiveProvider> logger,
            SqlQueueNotificationChannel channel, CancellationToken stoppingToken, string connectionString)
        {
            using (var sqlTableDependency = new EmarUpdateQueueSqlTableDependencyReactiveProvider(logger, connectionString, ThreadPoolScheduler.Instance))
            {
                IDisposable whenEntityRecordChangesSubscription =
                  sqlTableDependency.WhenEntityRecordChanges
                    // We only care about INSERTs to the table
                    .Where(c => c.ChangeType == ChangeType.Insert)
                    .Subscribe(async c =>
                    {
                        var insertedEntity = c.Entity;
                        //var oldValues = c.EntityOldValues;

                        logger.LogInformation(
                            $"Adding id #{insertedEntity.Id} ({insertedEntity.Entity} #{insertedEntity.ExternalId}) to the Channel.");

                        await channel.AddMessageAsync(insertedEntity);
                    });

                IDisposable whenStatusChangesSubscription =
                  sqlTableDependency.WhenStatusChanges
                    .Subscribe(status =>
                    {
                        logger.LogInformation($"SqlTableDependency Status {status}");
                    });

                sqlTableDependency.SubscribeToEntityChanges();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(60000, stoppingToken);
                }

                whenEntityRecordChangesSubscription.Dispose();
                whenStatusChangesSubscription.Dispose();
            }
        }
    }
}

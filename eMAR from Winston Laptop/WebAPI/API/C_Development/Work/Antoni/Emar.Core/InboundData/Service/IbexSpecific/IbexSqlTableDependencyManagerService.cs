using Emar.Core.Helpers;
using Emar.Core.InboundData.Model;
using Emar.Data;
using Emar.Data.IbexEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class IbexSqlTableDependencyManagerService : IIbexSqlTableDependencyManagerService
    {
        private readonly ILogger _logger;
        private readonly SqlQueueNotificationChannel _channel;
        private readonly IbexContext _context;
        private readonly IConfiguration _configuration;
        private DateTime _heartBeatDetected;
        private bool _sqlTableDependencyIsGood;

        public IbexSqlTableDependencyManagerService(ILogger<IIbexSqlTableDependencyManagerService> logger,
            SqlQueueNotificationChannel channel, IbexContext context, IConfiguration configuration)
        {
            _logger = logger;
            _channel = channel;
            _context = context;
            _configuration = configuration;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var queueMapper = new ModelToTableMapper<EmarUpdateQueue>()
                    .AddMapping(q => q.Id, "id")
                    .AddMapping(q => q.Entity, "entity")
                    .AddMapping(q => q.ExternalId, "external_id")
                    .AddMapping(q => q.EventDatetime, "event_datetime")
                    .AddMapping(q => q.InprocessDatetime, "inprocess_datetime")
                    .AddMapping(q => q.CompleteDatetime, "complete_datetime");

                var ibexSqlConnectionString = _configuration.GetConnectionString("IbexSqlConnection");

                using var queueTableDependency = new SqlTableDependency<EmarUpdateQueue>(ibexSqlConnectionString,
                    tableName: "emar_update_queue",
                    schemaName: "dbo",
                    mapper: queueMapper);

                queueTableDependency.OnChanged += Queue_OnChanged;
                queueTableDependency.OnError += Queue_OnError;
                queueTableDependency.Start();

                _logger.LogInformation("SqlTableDependency is started.");

                _sqlTableDependencyIsGood = true;

                while (!stoppingToken.IsCancellationRequested && _sqlTableDependencyIsGood)
                {
                    await Task.Delay(60000, stoppingToken);
                    if (stoppingToken.IsCancellationRequested)
                        continue;

                    if (_configuration.GetValue<bool>("BypassIdsSqlDependencyErrorMonitoring"))
                        continue;

                    // TODO: Test with normal no-activity heartbeat
                    // TODO: Test with SQL Service not responding
                    // TODO: Test with SQL Server taken off-line (take down VPN)
                    // If we've heard a heartbeat in the last 60 seconds, we're good
                    var ts = DateTime.Now - _heartBeatDetected;
                    //    if (ts < new TimeSpan(0, 0, 60))
                    //    {
                    //        _logger.LogInformation(
                    //            $"SqlTableDependency is working. Heartbeat detected {Math.Round(ts.TotalSeconds)} seconds ago.");
                    //        continue;
                    //    }

                    //    _logger.LogInformation("No queue activity in 60 seconds");

                    //    //// We haven't seen a heartbeat in the last 60 seconds.  Could be that it's a quiet time
                    //    //// of the day, so send a record to the queue and see if we get a heartbeat
                    //    //SendHeartbeatToQueue();
                    //    //await Task.Delay(100, stoppingToken);
                    //    //int waitingForHeartbeatCount = 0;

                    //    //while (DateTime.Now - _heartBeatDetected > new TimeSpan(0, 0, 60)
                    //    //       && waitingForHeartbeatCount < 20
                    //    //       && !stoppingToken.IsCancellationRequested)
                    //    //{
                    //    //    waitingForHeartbeatCount++;
                    //    //    await Task.Delay(100, stoppingToken);
                    //    //}

                    //    //// If we saw the heartbeat off the record we sent, we're good
                    //    //if (DateTime.Now - _heartBeatDetected < new TimeSpan(0, 0, 60))
                    //    //{
                    //    //    _logger.LogInformation(
                    //    //        $"SqlTableDependency is working. Synthetic heartbeat detected after {waitingForHeartbeatCount} delays.");

                    //    //    continue;
                    //    //}

                    //    //var msg =
                    //    //    $"SqlTableDependency is NOT working. Synthetic heartbeat not detected after {waitingForHeartbeatCount} delays.";
                    //    //_logger.LogError(msg);

                    //    //    throw new Exception(msg);
                }

                queueTableDependency.Stop();
            }
        }

        private void SendHeartbeatToQueue()
        {
            var dummyQueueRecord = new EmarUpdateQueue
            {
                Entity = InboundDataConstants.HeartbeatLabel,
                ExternalId = "-1"
            };
            _context.Add(dummyQueueRecord);
            _context.SaveChanges();
        }

        private async void Queue_OnChanged(object sender, RecordChangedEventArgs<EmarUpdateQueue> e)
        {
            // Set the heartbeat timestamp so we know the last time a message was received from the SQL Server
            _heartBeatDetected = DateTime.Now;

            // We only care about INSERTs to the table
            if (e.ChangeType != ChangeType.Insert)
                return;

            var changedEntity = e.Entity;

            _logger.LogInformation(
                $"Adding id #{changedEntity.Id} ({changedEntity.Entity} #{changedEntity.ExternalId}) to the Channel.");

            await _channel.AddMessageAsync(changedEntity);
        }

        private void Queue_OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogCritical($"SqlTableDependency threw an error:  {Utilities.ExtractExceptionMessages(e.Error)}");
            _sqlTableDependencyIsGood = false;

            //throw new NotImplementedException();
        }
    }
}

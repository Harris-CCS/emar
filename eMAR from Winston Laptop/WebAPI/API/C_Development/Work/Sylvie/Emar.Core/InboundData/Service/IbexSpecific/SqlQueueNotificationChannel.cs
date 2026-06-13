using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Emar.Data.IbexEntities;
using Microsoft.Extensions.Logging;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class SqlQueueNotificationChannel
    {
        private readonly ILogger<SqlQueueNotificationChannel> _logger;
        private const int MaxMessagesInChannel = 100;
        private readonly Channel<EmarUpdateQueue> _channel;

        public SqlQueueNotificationChannel(ILogger<SqlQueueNotificationChannel> logger)
        {
            var options = new BoundedChannelOptions(MaxMessagesInChannel)
            {
                SingleWriter = true,
                SingleReader = true
            };

            _channel = Channel.CreateBounded<EmarUpdateQueue>(options);

            _logger = logger;
        }

        internal async Task<bool> AddMessageAsync(EmarUpdateQueue emarUpdateRecord, CancellationToken ct = default)
        {
            while (await _channel.Writer.WaitToWriteAsync(ct) && !ct.IsCancellationRequested)
            {
                if (_channel.Writer.TryWrite(emarUpdateRecord))
                {
                    //_logger.LogInformation(
                    //    $"Added EmarUpdateQueue record for {emarUpdateRecord.Entity} #{emarUpdateRecord.ExternalId} to Channel");
                    return true;
                }
            }

            return false;
        }

        internal IAsyncEnumerable<EmarUpdateQueue> ReadAllAsync(CancellationToken ct = default) =>
            _channel.Reader.ReadAllAsync(ct);
    }
}

using Microsoft.AspNetCore.SignalR;
using NaeTime.Abstractions.Handlers;
using NaeTime.Abstractions.Models;
using NaeTime.Server.Hubs;
using NaeTime.Shared.Client;
using System.Collections.Concurrent;

namespace NaeTime.Server.Services
{
    public class RssiStreamBroadcastHandler : BackgroundService, IRssiStreamReadingHandler, IDisposable
    {
        private readonly long _waitPeriod = 250;
        private class StreamBuffer
        {
            public long MinTick { get; set; } = -1;
            public long MaxTick { get; set; } = -1;
            public ConcurrentBag<RssiStreamReading> Buffer { get; } = new ConcurrentBag<RssiStreamReading>();
        }
        private readonly IHubContext<ClientHub> _hubContext;
        private readonly ConcurrentQueue<RssiStreamReading> _readingQueue = new ConcurrentQueue<RssiStreamReading>();
        private readonly ConcurrentDictionary<Guid, StreamBuffer> _buffers = new ConcurrentDictionary<Guid, StreamBuffer>();

        public RssiStreamBroadcastHandler(IHubContext<ClientHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_waitPeriod));

            while (stoppingToken.IsCancellationRequested)
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                if (!stoppingToken.IsCancellationRequested)
                {
                    var toSend = new List<RssiStreamReadingDto>();
                    while (_readingQueue.TryDequeue(out var reading) && !stoppingToken.IsCancellationRequested)
                    {
                        var streamBuffer = _buffers.GetOrAdd(reading.StreamId, new StreamBuffer() { MaxTick = reading.Tick, MinTick = reading.Tick });

                        if (reading.Tick > streamBuffer.MaxTick)
                        {
                            streamBuffer.Buffer.Add(reading);
                            streamBuffer.MaxTick = reading.Tick;

                            var tickDifference = reading.Tick - streamBuffer.MinTick;
                            if (tickDifference > _waitPeriod)
                            {
                                var dto = new RssiStreamReadingDto()
                                {
                                    StreamId = reading.StreamId,
                                    Tick = (long)streamBuffer.Buffer.Average(x => x.Tick),
                                    Value = (int)streamBuffer.Buffer.Average(x => x.Value),
                                };

                                toSend.Add(dto);

                                streamBuffer.Buffer.Clear();
                                streamBuffer.MinTick = reading.Tick;
                            }
                        }
                    }

                    if (toSend.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveReading", toSend);
                    }
                }
            }
        }
        public void HandleReading(RssiStreamReading reading) => _readingQueue.Enqueue(reading);



    }
}

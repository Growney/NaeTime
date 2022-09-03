using NaeTime.Node.Abstractions;
using NaeTime.Shared.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Services
{
    public class RssiStreamSender : IHostedService, ICommunicationManager
    {
        private Guid _nodeId;
        private string? _serviceUri;
        private int _transmissionDelay = 1000;
        private int _rssiRetryCount = 0;

        private readonly IRssiStreamAggregationQueue _queue;
        private readonly ILogger<RssiStreamSender> _logger;
        private readonly IHttpClientFactory _clientFactory;

        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();


        public RssiStreamSender(IHttpClientFactory clientFactory, IRssiStreamAggregationQueue queue, ILogger<RssiStreamSender> logger)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Configure(Guid nodeId, string serviceUri, int? transmissionDelay, int? rssiRetryCount)
        {
            _nodeId = nodeId;
            _serviceUri = serviceUri;
            _transmissionDelay = transmissionDelay ?? _transmissionDelay;
            _rssiRetryCount = rssiRetryCount ?? _rssiRetryCount;
        }
        private async void ProcessQueue()
        {
            _logger.LogInformation("Starting Rssi Stream Sender");
            try
            {
                while (!_cancel.Token.IsCancellationRequested)
                {

                    if (_queue.HasValues)
                    {
                        if (_serviceUri != null)
                        {
                            if (_nodeId != Guid.Empty)
                            {
                                var streamDtos = GetQueueStreamDtos();
                                var nodeDto = GetNodeDto(streamDtos);
                                try
                                {

                                    await SendNodeDto(_serviceUri, nodeDto);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Error sending rssi values", ex);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Unable to send Rssi values as the node id is not set");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Unable to send Rssi values as the service uri is not set");
                        }
                    }

                    await Task.Delay(_transmissionDelay);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error in Rssi Stream sender, {ex} : {ex.Message}", ex);
            }
            _logger.LogInformation("Stopping Rssi Stream sender");
        }
        private async Task SendNodeDto(string serverUri, NodeRssiStreamValuesDto dto)
        {
            var client = _clientFactory.CreateClient("Default");
            client.BaseAddress = new Uri(serverUri);
            await client.PostAsJsonAsync("/node/values", dto);
        }
        private NodeRssiStreamValuesDto GetNodeDto(IEnumerable<RssiStreamValuesDto> values)
            => new NodeRssiStreamValuesDto()
            {
                NodeId = _nodeId,
                Values = values.ToList()
            };
        private IEnumerable<RssiStreamValuesDto> GetQueueStreamDtos()
        {
            var streamDictionary = new Dictionary<Guid, RssiStreamValuesDto>();
            var queueReadings = _queue.Dequeue();
            foreach (var reading in queueReadings)
            {
                if (!streamDictionary.TryGetValue(reading.StreamId, out var dto))
                {
                    dto = new RssiStreamValuesDto()
                    {
                        StreamId = reading.StreamId,
                        RssiValues = new List<RssiValueDto>()
                    };
                    streamDictionary.Add(reading.StreamId, dto);
                }
                var readingDto = new RssiValueDto()
                {
                    Tick = reading.Tick,
                    Value = reading.Value,
                };

                dto.RssiValues!.Add(readingDto);
            }

            return streamDictionary.Values;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var thread = new Thread(new ThreadStart(ProcessQueue));

            thread.Start();
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancel.Cancel();

            return Task.CompletedTask;
        }
    }
}

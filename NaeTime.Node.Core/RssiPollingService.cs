using Microsoft.Extensions.Hosting;
using NaeTime.Node.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gpio;
using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.Core
{
    public class RssiPollingService : IHostedService, IRssiPollingService
    {
        private readonly IEnumerable<IRssiReceiverManager> _managers;
        private readonly IRssiStreamAggregationQueue _queue;
        private readonly ILogger<RssiPollingService> _logger;
        private readonly INodeTimeProvider _nodeTimeProvider;
        private readonly ManualResetEvent _pauseEvent;
        private readonly ManualResetEvent _resumeEvent;

        private bool _pauseWaiting;
        private bool _receiversChanged;

        private bool _shouldPollingStop;
        private bool _shouldStop;

        public RssiPollingService(IEnumerable<IRssiReceiverManager> managers, IRssiStreamAggregationQueue queue, INodeTimeProvider nodeTimeProvider, ILogger<RssiPollingService> logger)
        {
            _managers = managers;
            _queue = queue;
            _nodeTimeProvider = nodeTimeProvider;
            _logger = logger;
            _pauseEvent = new ManualResetEvent(false);
            _resumeEvent = new ManualResetEvent(true);
        }

        public Task PausePolling()
        {
            _resumeEvent.Reset();
            _shouldPollingStop = true;
            var waitTask = Task.Run(() => _pauseEvent.WaitOne());
            _pauseWaiting = true;
            return waitTask;
        }

        public void ResumePolling()
        {
            _shouldPollingStop = false;
            _resumeEvent.Set();
            _pauseWaiting = false;
        }
        private void PollRssi()
        {
            _logger.LogInformation("Starting rssi polling service");
            var enabledReceivers = new List<IRssiReceiver>();
            while (!_shouldStop)
            {
                if (_pauseWaiting)
                {
                    _pauseEvent.Set();
                }

                _resumeEvent.WaitOne();
                _pauseEvent.Reset();

                if (_receiversChanged)
                {
                    enabledReceivers.Clear();
                    foreach (var manager in _managers)
                    {
                        enabledReceivers.AddRange(manager.GetEnabledReceivers());
                    }
                    _receiversChanged = false;
                    _shouldPollingStop = _shouldStop;
                }

                if (enabledReceivers.Count > 0)
                {
                    _logger.LogInformation($"Found {enabledReceivers.Count} receivers, Starting polling loop");
                    while (!_shouldPollingStop)
                    {
                        int receiverCount = enabledReceivers.Count;
                        for (int i = 0; i < receiverCount; i++)
                        {
                            var receiver = enabledReceivers[i];
                            if (receiver.CurrentStream != null)
                            {

                                var rssi = receiver.GetRssi();
                                var rssiValue = new RssiStreamReading(receiver.CurrentStream.Id, _nodeTimeProvider.ElapsedMilliseconds, rssi);
                                _queue.Enqueue(rssiValue);
                            }


                        }
                    }
                    _logger.LogInformation($"Polling loop finished");
                }
                else
                {
                    _logger.LogTrace($"Sleeping while we wait for some receivers");
                    Thread.Sleep(500);
                }
            }

            _logger.LogInformation("Stopping rssi polling service");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var manager in _managers)
            {
                manager.OnReceiversChanged += Manager_OnReceiversChanged;
            }

            var pollingThread = new Thread(new ThreadStart(PollRssi));
            pollingThread.Start();

            return Task.CompletedTask;
        }

        private void Manager_OnReceiversChanged(object? sender, EventArgs e)
        {
            _logger.LogInformation("Rssi Polling Service has been told to update its receivers");
            _receiversChanged = true;
            _shouldPollingStop = true;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _resumeEvent.Set();
            _shouldPollingStop = true;
            _shouldStop = true;

            return Task.CompletedTask;
        }

    }
}

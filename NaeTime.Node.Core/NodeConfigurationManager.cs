using Microsoft.Extensions.Hosting;
using NaeTime.Node.Abstractions;
using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NaeTime.Node.Core
{
    public class NodeConfigurationManager : IHostedService, INodeConfigurationManager
    {
        private readonly IRssiPollingService _rssiPollingService;
        private readonly IRX5808ReceiverManager _rX5808ReceiverManager;
        private readonly IEnumerable<IRssiReceiverManager> _rssiReceiverManagers;
        private readonly ICommunicationManager _communicationManager;
        private readonly IRssiStreamAggregationQueue _rssiStreamAggregationQueue;

        private readonly string _configFilePath;
        private readonly string _configDirectory;

        public NodeConfigurationManager(IRssiPollingService rssiPollingService, IRX5808ReceiverManager rX5808ReceiverManager, IEnumerable<IRssiReceiverManager> rssiReceiverManagers, ICommunicationManager communicationManager, IRssiStreamAggregationQueue rssiStreamAggregationQueue)
        {
            _rssiPollingService = rssiPollingService ?? throw new ArgumentNullException(nameof(rssiPollingService));
            _rX5808ReceiverManager = rX5808ReceiverManager ?? throw new ArgumentNullException(nameof(rX5808ReceiverManager));
            _rssiReceiverManagers = rssiReceiverManagers ?? throw new ArgumentNullException(nameof(rssiReceiverManagers));
            _communicationManager = communicationManager ?? throw new ArgumentNullException(nameof(communicationManager));
            _rssiStreamAggregationQueue = rssiStreamAggregationQueue ?? throw new ArgumentNullException(nameof(rssiStreamAggregationQueue));

            _configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NaeTime");
            _configFilePath = Path.Combine(_configDirectory, "config.json");
        }

        public async Task ApplyConfigurationAsync(NodeConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.ServerUri))
            {
                throw new ArgumentNullException(nameof(configuration.ServerUri));
            }

            foreach (var receiverManager in _rssiReceiverManagers)
            {
                var enabledReceivers = receiverManager.GetEnabledReceivers();
                if (enabledReceivers.Count() > 0)
                {
                    throw new InvalidOperationException("Cannot set configuration as RSSI polling is enabled");
                }
            }


            try
            {
                await _rssiPollingService.PausePolling();
                _rssiStreamAggregationQueue.Clear();
                _communicationManager.Configure(configuration.NodeId, configuration.ServerUri, configuration.RssiTransmissionDelay, configuration.RssiRetryCount);
                await _rX5808ReceiverManager.SetupReceivers(configuration.RX5808Receivers);
            }
            finally
            {
                _rssiPollingService.ResumePolling();
            }

        }



        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var fileContents = await File.ReadAllTextAsync(_configFilePath);
            if (!string.IsNullOrWhiteSpace(fileContents))
            {
                try
                {
                    var configuration = JsonSerializer.Deserialize<NodeConfiguration>(fileContents);

                    if (configuration != null)
                    {
                        await ApplyConfigurationAsync(configuration);
                    }
                }
                catch
                {

                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


        public Task StoreConfigurationAsync(NodeConfiguration configuration)
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            var configJson = JsonSerializer.Serialize(configuration);

            return File.WriteAllTextAsync(_configFilePath, configJson);
        }
        public async Task<NodeConfiguration?> GetConfigurationAsync()
        {
            if (File.Exists(_configFilePath))
            {
                var configJson = await File.ReadAllTextAsync(_configFilePath);
                return JsonSerializer.Deserialize<NodeConfiguration>(configJson);
            }
            return null;
        }
    }
}

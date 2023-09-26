using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.Domain;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NaeTime.Node.Services;

public class NodeManager : BackgroundService
{
    private const int c_readingGroupSize = 500;
    private readonly INodeDeviceFactory _nodeDeviceFactory;
    private readonly INodeClientFactory _nodeClientFactory;
    private readonly INodeConfigurationManager _nodeConfigurationManager;
    private readonly ILogger<NodeManager> _logger;

    public NodeManager(INodeConfigurationManager nodeConfigurationManager, INodeClientFactory clientFactory, INodeDeviceFactory nodeDeviceFactory, ILogger<NodeManager> logger)
    {
        _nodeConfigurationManager = nodeConfigurationManager;
        _nodeClientFactory = clientFactory;
        _nodeDeviceFactory = nodeDeviceFactory;
        _logger = logger;
    }

    private Task RunRssiScanAsync(Stopwatch stopwatch, ConcurrentQueue<RssiReading> queue, ITunedRssiDevice[] devices, CancellationToken stoppingToken)
        => Task.Run(() =>
        {
            bool shouldScanRssi = devices.Length > 0 && !stoppingToken.IsCancellationRequested;
            if (shouldScanRssi)
            {
                _logger.LogInformation("{enabledDevicesCount} enabled devices found", devices.Length);
                _logger.LogInformation("Starting RSSI Scan");
                while (!stoppingToken.IsCancellationRequested)
                {
                    for (int i = 0; i < devices.Length; i++)
                    {
                        var device = devices[i];
                        if (device.IsEnabled)
                        {
                            int rssiValue = device.RssiCommunication.ReadRssi();
                            queue.Enqueue(new RssiReading(stopwatch.ElapsedMilliseconds, rssiValue, device.TunningCommunication.TunedFrequency, device.DeviceId));
                        }
                    }
                }

                _logger.LogInformation("Stopping RSSI Scan");
            }
        });
    private Task RunRssiGroupAndSendAsync(INodeClient client, Guid nodeId, ConcurrentQueue<RssiReading> queue, CancellationToken stoppingToken)
        => Task.Run(async () =>
        {
            var averagedReadings = new Dictionary<byte, AveragedRssiReading>();
            var groupedReadings = new Dictionary<byte, GroupedRssiReading>();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (queue.TryDequeue(out var reading))
                {
                    if (!averagedReadings.TryGetValue(reading.DeviceId, out var readingAverage))
                    {
                        readingAverage = new AveragedRssiReading();
                        averagedReadings.Add(reading.DeviceId, readingAverage);
                    }

                    if (readingAverage.IsNewAverage(reading.Tick, reading.Value, reading.Frequency, out var averaged))
                    {
                        var averagedReading = new RssiReading(reading.Tick, averaged, reading.Frequency, reading.DeviceId);
                        if (!groupedReadings.TryGetValue(reading.DeviceId, out var readingGroup))
                        {
                            readingGroup = new GroupedRssiReading(c_readingGroupSize);
                            groupedReadings.Add(reading.DeviceId, readingGroup);
                        }

                        if (readingGroup.IsNewGroup(averagedReading, out var readings))
                        {
                            await client.SendReadingsAsync(nodeId, reading.DeviceId, reading.Frequency, readings);
                        }
                    }
                }
                else
                {
                    await Task.Delay(500);
                }

            }
        });

    public async Task SetConfiguration(NodeConfiguration nodeConfiguration)
    {
        var nodeDevices = await _nodeDeviceFactory.GetNodeDevices(nodeConfiguration);

        var nodeDictionary = new Dictionary<byte, ITunedRssiDevice>();

        foreach (var tunedDevice in nodeDevices.TunedRssiDevices)
        {
            if (!nodeDictionary.ContainsKey(tunedDevice.DeviceId))
            {
                nodeDictionary.Add(tunedDevice.DeviceId, tunedDevice);
            }
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int configurationRetryAttempts = 0;
        NodeConfiguration? configuration = await _nodeConfigurationManager.GetCurrentConfigurationAsync();

        CancellationTokenSource resetSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        _nodeConfigurationManager.OnConfigurationChanged += x =>
        {
            configurationRetryAttempts = 0;
            configuration = x;
            resetSource.Cancel();
            resetSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        };

        _logger.LogInformation("Starting Node manager");
        while (!stoppingToken.IsCancellationRequested)
        {
            if (configuration != null)
            {
                try
                {
                    await RunNodeForConfigurationAsync(configuration, resetSource.Token);

                    if (!resetSource.IsCancellationRequested)
                    {
                        configurationRetryAttempts++;
                        TimeSpan configurationRetry = TimeSpan.FromSeconds(configurationRetryAttempts * 2);
                        _logger.LogInformation("Node configuration stopped unexpectedly. Will retry configuration in {retryTimeout} seconds", configurationRetry.TotalSeconds);
                        await Task.Delay(configurationRetry, resetSource.Token);
                    }
                }
                catch (Exception ex)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        configurationRetryAttempts++;
                        TimeSpan configurationRetry = TimeSpan.FromSeconds(configurationRetryAttempts * 2);
                        _logger.LogError(ex, "Error when running for configuration. Will retry configuration in {retryTimeout} seconds", configurationRetry.TotalSeconds);
                        await Task.Delay(configurationRetry, resetSource.Token);
                    }
                }
            }
            else
            {
                _logger.LogInformation("No configuration set sleeping manager");
                await Task.Delay(TimeSpan.FromSeconds(10), resetSource.Token);
            }

        }



    }

    private Task RunNodeForConfigurationAsync(NodeConfiguration configuration, CancellationToken configurationStoppingToken)
        => Task.Run(async () =>
        {
            if (configurationStoppingToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation("Starting configuration execution for node id {nodeId}", configuration.NodeId);

            using var devices = await _nodeDeviceFactory.GetNodeDevices(configuration);

            var tunedDeviceArray = devices.TunedRssiDevices.ToArray();

            if (tunedDeviceArray.Length == 0)
            {
                return;
            }

            _logger.LogInformation("Node id {nodeId} has {tunedDeviceCount} tuned devices", configuration.NodeId, tunedDeviceArray.Length);

            _logger.LogInformation("Creating node client");
            var nodeClient = await _nodeClientFactory.CreateNodeClientAsync(configuration, configurationStoppingToken);

            if (configurationStoppingToken.IsCancellationRequested)
            {
                return;
            }

            await nodeClient.SendInitializedAsync(configuration);

            var sessionId = Guid.NewGuid();
            _logger.LogInformation("Starting new node timer session {sessionId}", sessionId);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await nodeClient.SendTimerStartAsync(configuration.NodeId, sessionId);

            var readingQueue = new ConcurrentQueue<RssiReading>();

            await Task.WhenAll(
                RunRssiGroupAndSendAsync(nodeClient, configuration.NodeId, readingQueue, configurationStoppingToken),
                RunRssiScanAsync(stopwatch, readingQueue, tunedDeviceArray, configurationStoppingToken));
        });
}

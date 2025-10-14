using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeManager(INodeConnectionFactory connectionFactory, ISoftwareTimer softwareTimer) : IHostedService
{
    private readonly ISoftwareTimer _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    private readonly INodeConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    private readonly ConcurrentDictionary<Guid, NodeConnection> _hardwareProcesses = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<Guid, NodeConnection> device in _hardwareProcesses)
        {
            await device.Value.Stop().ConfigureAwait(false);
        }
    }
}
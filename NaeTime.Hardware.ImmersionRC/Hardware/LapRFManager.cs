using Microsoft.Extensions.Hosting;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager(ILapRFConnectionFactory connectionFactory) : IHostedService
{
    private readonly ILapRFConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<Guid, LapRFConnection> device in _hardwareProcesses)
        {
            await device.Value.Stop().ConfigureAwait(false);
        }

    }
}

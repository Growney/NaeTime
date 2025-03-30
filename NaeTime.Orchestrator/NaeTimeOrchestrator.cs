using Microsoft.Extensions.DependencyInjection;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Orchestrator;

public class NaeTimeOrchestrator : INaeTimeOrchestrator, IDisposable
{
    public IHardwareOrchestrator Hardware { get; }
    public IManagementOrchestrator Management { get; }
    public IOpenPracticeOrchestrator OpenPractice { get; }
    public ITimingOrchestrator Timing { get; }

    private readonly IServiceScope _serviceScope;
    private readonly INaeTimeOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public NaeTimeOrchestrator(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();

        _orchestratorPersistence = _serviceScope.ServiceProvider.GetRequiredService<INaeTimeOrchestratorPersistence>();
        _distribution = _serviceScope.ServiceProvider.GetRequiredService<INaeTimeOrchestratorDistribution>();

        ILapRFManager lapRFManager = _serviceScope.ServiceProvider.GetRequiredService<ILapRFManager>();
        INodeManager nodeManager = _serviceScope.ServiceProvider.GetRequiredService<INodeManager>();
        INaeTimePersistence persistence = _serviceScope.ServiceProvider.GetRequiredService<INaeTimePersistence>();

        Hardware = new HardwareOrchestrator(lapRFManager, nodeManager, persistence, _orchestratorPersistence.Hardware, _distribution);
        Management = new ManagementOrchestrator(_orchestratorPersistence.Management, persistence, _distribution);
        OpenPractice = new OpenPracticeOrchestrator(_orchestratorPersistence.OpenPractice, _distribution, Hardware, persistence);
        Timing = new TimingOrchestrator(_orchestratorPersistence.Timing, _distribution);
    }

    public async Task CommitAsync()
    {
        await _orchestratorPersistence.CommitAsync();
        await _distribution.CommitAsync();
    }

    public void Dispose() => _serviceScope.Dispose();
}

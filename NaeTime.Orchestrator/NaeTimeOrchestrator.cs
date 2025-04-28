using Microsoft.Extensions.DependencyInjection;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Orchestrator;

public class NaeTimeOrchestrator : INaeTimeOrchestrator
{
    public IHardwareOrchestrator Hardware { get; }
    public IManagementOrchestrator Management { get; }
    public IOpenPracticeOrchestrator OpenPractice { get; }
    public ITimingOrchestrator Timing { get; }

    private readonly INaeTimeOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public NaeTimeOrchestrator(IServiceProvider serviceProvider)
    {
        _orchestratorPersistence = serviceProvider.GetRequiredService<INaeTimeOrchestratorPersistence>();
        _distribution = serviceProvider.GetRequiredService<INaeTimeOrchestratorDistribution>();

        ILapRFManager lapRFManager = serviceProvider.GetRequiredService<ILapRFManager>();
        INodeManager nodeManager = serviceProvider.GetRequiredService<INodeManager>();
        INaeTimePersistence persistence = serviceProvider.GetRequiredService<INaeTimePersistence>();

        Hardware = new HardwareOrchestrator(lapRFManager, nodeManager, persistence, _orchestratorPersistence.Hardware, _distribution);
        Management = new ManagementOrchestrator(_orchestratorPersistence.Management, Hardware, _distribution);
        OpenPractice = new OpenPracticeOrchestrator(_orchestratorPersistence.OpenPractice, _distribution, Hardware, Management);
        Timing = new TimingOrchestrator(_orchestratorPersistence.Timing, _distribution);
    }

    public async Task CommitAsync()
    {
        await _orchestratorPersistence.CommitAsync();
        await _distribution.CommitAsync();
    }
}

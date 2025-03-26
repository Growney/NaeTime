using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class NaeTimeOrchestratorPersistence : INaeTimeOrchestratorPersistence
{
    public IHardwareOrchestratorPersistence Hardware { get; }
    public IManagementOrchestratorPersistence Management { get; }
    public IOpenPracticeOrchestratorPersistence OpenPractice { get; }
    public ITimingOrchestratorPersistence Timing { get; }

    private readonly NaeTimeDbContext _context;

    public NaeTimeOrchestratorPersistence(NaeTimeDbContext context)
    {
        _context = context;
        Hardware = new HardwareOrchestratorPersistence(context);
        Management = new ManagementOrchestratorPersistence(context);
        OpenPractice = new OpenPracticeOrchestratorPersistence(context);
        Timing = new TimingOrchestratorPersistence(context);
    }

    public Task CommitAsync() => _context.SaveChangesAsync();
}

using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class TimingOrchestratorPersistence : ITimingOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;

    public TimingOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}

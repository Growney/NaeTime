using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class OpenPracticeOrchestratorPersistence : IOpenPracticeOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;

    public OpenPracticeOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}

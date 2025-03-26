using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class HardwareOrchestratorPersistence : IHardwareOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;
    public HardwareOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task ConnectTimer(Guid timerId, DateTime connectionTime)
    {
        _dbContext.TimerStatuses.Add(new NaeTime.Persistence.EntityFramework.Models.TimerStatus()
        {
            Id = Guid.NewGuid(),
            ConnectionStatusChanged = connectionTime,
            TimerId = timerId,
            WasConnected = true
        });

        return Task.CompletedTask;
    }

    public Task DisconnectTimer(Guid timerId, DateTime disconnectionTime)
    {
        _dbContext.TimerStatuses.Add(new NaeTime.Persistence.EntityFramework.Models.TimerStatus()
        {
            Id = Guid.NewGuid(),
            ConnectionStatusChanged = disconnectionTime,
            TimerId = timerId,
            WasConnected = false
        });

        return Task.CompletedTask;
    }
}

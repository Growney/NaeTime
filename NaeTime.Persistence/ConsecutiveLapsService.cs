using NaeTime.Messages.Events.OpenPractice;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class ConsecutiveLapsService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public ConsecutiveLapsService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task When(OpenPracticeConsecutiveLapRecordRemoved removed)
    {
        var repository = await _repositoryFactory.CreateLapRecordRepository().ConfigureAwait(false);

        await repository.RemoveConsecutiveLapRecord(removed.SessionId, removed.PilotId, removed.LapCap).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeConsecutiveLapRecordImproved improved)
    {
        var repository = await _repositoryFactory.CreateLapRecordRepository().ConfigureAwait(false);

        await repository.AddOrUpdateConsecutiveLapRecord(improved.SessionId, improved.PilotId, improved.LapCap, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps).ConfigureAwait(false);
    }
    public async Task When(OpenPracticeConsecutiveLapRecordReduced reduced)
    {
        var repository = await _repositoryFactory.CreateLapRecordRepository().ConfigureAwait(false);

        await repository.AddOrUpdateConsecutiveLapRecord(reduced.SessionId, reduced.PilotId, reduced.LapCap, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps).ConfigureAwait(false);
    }
}

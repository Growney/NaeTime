namespace NaeTime.Persistence.Abstractions.Timing;

public interface ITimingRepository
{
    public Task<IEnumerable<ActiveLaneConfiguration>> GetActiveLaneConfigurations();
    public Task<IEnumerable<LaneActiveTimings>> GetSessionActiveTimings(Guid sessionId);
}

namespace NaeTime.Persistence.Abstractions.Timing;

public interface ITimingRepository
{
    public Task<IEnumerable<LaneActiveTimings>> GetSessionActiveTimings(Guid sessionId);
    public Task<LaneActiveTimings?> GetSessionLaneActiveTimings(Guid sessionId, byte laneId);
}

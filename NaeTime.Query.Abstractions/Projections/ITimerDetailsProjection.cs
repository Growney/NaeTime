namespace NaeTime.Query.Abstractions.Projections;
public interface ITimerDetailsProjection
{
    public Models.TimerDetails GetDetails(Guid timerId);
    public Models.TimerLaneDetails GetLaneDetails(Guid timerId, byte laneId);
}

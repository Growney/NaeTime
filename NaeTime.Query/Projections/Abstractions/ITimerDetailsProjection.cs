namespace NaeTime.Query.Projections.Abstractions;
public interface ITimerDetailsProjection
{
    public NaeTime.Query.Abstractions.Models.TimerDetails GetDetails(Guid timerId);
    public NaeTime.Query.Abstractions.Models.TimerLaneDetails GetLaneDetails(Guid timerId, byte laneId);
}

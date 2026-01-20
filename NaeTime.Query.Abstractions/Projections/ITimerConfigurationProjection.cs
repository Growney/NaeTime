namespace NaeTime.Query.Abstractions.Projections;
public interface ITimerConfigurationProjection
{
    public IEnumerable<Models.DesiredImmersionRCLapRFLane> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId);
    public IEnumerable<Models.DesiredNaeTimeNodeLane> GetActiveNaeTimeNodeLanesConfiguration(Guid timerId);
}

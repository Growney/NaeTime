namespace NaeTime.Query.Projections.Abstractions;
public interface ITimerConfigurationProjection
{
    public IEnumerable<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId);
}

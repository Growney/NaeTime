
namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeSessionRequested(Guid SessionId, DateTime UtcTime, long SoftwareTime, IEnumerable<PracticeSessionRequested.LaneConfiguration> LaneConfigurations)
{
    public record LaneConfiguration(byte Lane, Guid? PilotId, int FrequencyInMhz);
}
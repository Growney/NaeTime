using NaeTime.Timing.Messages.Models;

namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeSessionRequested(Guid SessionId, DateTime UtcTime, long SoftwareTime, IEnumerable<LaneConfiguration> LaneConfigurations);
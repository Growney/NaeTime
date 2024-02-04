using NaeTime.Messages.Models;

namespace NaeTime.Messages.Responses;
public record TimerRadioFrequencyResponse(Guid TimerId, IEnumerable<LaneRadioFrequency> LaneFrequencies);

using NaeTime.Timing.Abstractions.Models;

namespace NaeTime.Timing.Abstractions.Notifications;
public record TimerRadioFrequenciesAquired(Guid TimerId, IEnumerable<TimerRadioFrequencyChannel> Channels);
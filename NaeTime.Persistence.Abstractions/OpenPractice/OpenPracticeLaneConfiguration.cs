using NaeTime.Persistence.Abstractions.Hardware;

namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record OpenPracticeLaneConfiguration(byte Lane, Guid? PilotId, LaneConfiguration? LaneConfiguration);
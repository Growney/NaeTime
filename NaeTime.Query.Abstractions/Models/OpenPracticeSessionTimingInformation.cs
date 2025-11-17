namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionTimingInformation(
    IDictionary<Guid, IEnumerable<OpenPracticeTimingMoment>> Moments,
    IDictionary<Guid, IDictionary<Guid,OpenPracticeDetection>> PilotDetections,
    IDictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>> PilotLapGroups,
    IDictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> PilotLapRecords,
    IDictionary<uint, IEnumerable<OpenPracticeLapRecord>> SessionLapRecords);
namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionTimingInformation(IDictionary<Guid, IEnumerable<OpenPracticeDetection>> PilotDetections,
    IDictionary<Guid, IEnumerable<IEnumerable<OpenPracticeLap>>> PilotLapGroups,
    IDictionary<Guid, IDictionary<uint, OpenPracticeLapRecord>> PilotLapRecords,
    IDictionary<uint, IEnumerable<OpenPracticeLapRecord>> SessionLapRecords);
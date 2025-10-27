namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionPilotTimingInfo(IEnumerable<OpenPracticeDetection> Detections, IEnumerable<IEnumerable<OpenPracticeLap>> LapGroups, IDictionary<uint, OpenPracticeLapRecord> LapRecords);

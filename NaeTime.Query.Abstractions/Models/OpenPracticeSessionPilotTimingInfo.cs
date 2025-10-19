namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionPilotTimingInfo(IEnumerable<OpenPracticeDetection> Detections, IEnumerable<OpenPracticeLap> Laps, IDictionary<uint, OpenPracticeLapRecord> LapRecords);

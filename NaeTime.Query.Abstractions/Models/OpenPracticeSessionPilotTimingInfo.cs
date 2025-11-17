namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionPilotTimingInfo(
    IEnumerable<OpenPracticeTimingMoment> Moments, 
    IDictionary<Guid,OpenPracticeDetection> IndexedDetections, 
    IEnumerable<IEnumerable<OpenPracticeLap>> LapGroups, 
    IDictionary<uint, OpenPracticeLapRecord> LapRecords)
{
    public IEnumerable<OpenPracticeDetection> Detections => IndexedDetections.Values;
}

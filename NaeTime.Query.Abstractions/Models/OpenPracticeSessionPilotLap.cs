namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionPilotLap(Guid Id, OpenPracticeDetection StartDetection, OpenPracticeDetection? EndDetection, bool IsIncluded);
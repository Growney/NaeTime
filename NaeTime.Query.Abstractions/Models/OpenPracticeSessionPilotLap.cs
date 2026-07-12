namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeSessionPilotLap(Guid Id, Guid SessionId, Guid PilotId, Detection StartDetection, Detection? EndDetection, bool IsIncluded);
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface IOpenPracticeQueryHandler
{
    Task<OpenPracticeSession?> GetByIdAsync(Guid id);
    Task<OpenPracticeSessionTimingInformation> GetTimingInformation(Guid sessionId, Guid trackId, TimeSpan minimumLapTime, TimeSpan maximumLapTime);
}

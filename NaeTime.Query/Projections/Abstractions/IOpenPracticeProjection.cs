using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface IOpenPracticeProjection
{
    OpenPracticeSession? GetSession(Guid sessionId);
}
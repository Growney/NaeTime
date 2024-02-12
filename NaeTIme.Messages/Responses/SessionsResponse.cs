using static NaeTime.Messages.Responses.SessionsResponse;

namespace NaeTime.Messages.Responses;
public record SessionsResponse(IEnumerable<Session> Sessions)
{
    public record Session(Guid Id, string? Name, SessionType Type);
    public enum SessionType
    {
        OpenPractice
    }
}

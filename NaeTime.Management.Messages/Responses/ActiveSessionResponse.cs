namespace NaeTime.Management.Messages.Responses;
public record ActiveSessionResponse(Guid SessionId, ActiveSessionResponse.SessionType Type)
{
    public enum SessionType
    {
        OpenPractice,
    }
}
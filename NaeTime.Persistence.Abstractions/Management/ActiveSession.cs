namespace NaeTime.Persistence.Abstractions.Management;
public record ActiveSession(Guid SessionId, ActiveSession.SessionType Type)
{
    public enum SessionType
    {
        OpenPractice,
    }
}
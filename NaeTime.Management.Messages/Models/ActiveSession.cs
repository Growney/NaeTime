namespace NaeTime.Management.Messages.Models;
public record ActiveSession(Guid SessionId, ActiveSession.SessionType Type)
{
    public enum SessionType
    {
        OpenPractice,
    }
}
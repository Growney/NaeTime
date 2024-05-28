namespace NaeTime.Management.Messages;
public record SessionActivated(Guid SessionId, SessionActivated.SessionType Type)
{
    public enum SessionType
    {
        OpenPractice,
    }
}
namespace NaeTime.Messages.Events.Activation;
public record SessionActivated(Guid SessionId, SessionActivated.SessionType Type)
{
    public enum SessionType
    {
        OpenPractice,
    }
}
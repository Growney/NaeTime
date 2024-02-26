namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeLapDisputed(Guid SessionId, Guid LapId, Guid PilotId, OpenPracticeLapDisputed.OpenPracticeLapStatus ActualStatus)
{
    public enum OpenPracticeLapStatus
    {
        Invalid,
        Completed
    }
}

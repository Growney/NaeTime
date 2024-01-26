namespace NaeTime.Client.Razor.Lib.Models;
public class TimedGate
{
    public TimedGate(Guid timerId)
    {
        TimerId = timerId;
    }

    public Guid TimerId { get; }
}

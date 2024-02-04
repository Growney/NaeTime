namespace NaeTime.Timing.Messages.Models;
public record Track(Guid TrackId, long MinimumLapMilliseconds, long MaximumLapMilliseconds, IEnumerable<Guid> Timers)
{
    public int GetTimerPosition(Guid timerId)
    {
        int position = 0;
        foreach (var timer in Timers)
        {
            if (timer == timerId)
            {
                return position;
            }
            position++;
        }
        return -1;
    }
}

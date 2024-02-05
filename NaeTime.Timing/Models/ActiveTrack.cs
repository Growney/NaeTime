namespace NaeTime.Timing.Models;
public record ActiveTrack(Guid Id, long MinimumLapMilliseconds, long MaximumLapMilliseconds, IEnumerable<Guid> Timers)
{
    public int GetTimerPosition(Guid timerId)
    {
        int position = -1;
        foreach (var timer in Timers)
        {
            position++;
            if (timer == timerId)
            {
                return position;
            }
        }
        return -1;
    }
}
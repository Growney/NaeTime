namespace NaeTime.Timing.Models;
public record ActiveTrack(IEnumerable<Guid> Timers)
{
    public int GetTimerPosition(Guid timerId)
    {
        int position = -1;
        foreach (Guid timer in Timers)
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
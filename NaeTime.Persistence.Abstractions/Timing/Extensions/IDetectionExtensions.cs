namespace NaeTime.Persistence.Abstractions.Timing.Extensions;
public static class IDetectionExtensions
{
    public static long MillisecondsBetween(this IDetection start, IDetection? end)
    {
        if (end == null)
        {
            return 0;
        }

        if (start.TimerId != end.TimerId)
        {
            return (long)(end.UtcTime - start.UtcTime).TotalMilliseconds;
        }

        if (start.HardwareTime.HasValue && end.HardwareTime.HasValue)
        {
            return (long)(end.HardwareTime.Value - start.HardwareTime.Value);
        }

        return end.SoftwareTime - start.SoftwareTime;
    }
    public static TimeSpan TimeBetween(this IDetection start, IDetection? end) => TimeSpan.FromMilliseconds(start.MillisecondsBetween(end));

    public static int Compare(this IDetection start, IDetection? end)
    {
        if (end == null)
        {
            return 1;
        }

        if (start.TimerId != end.TimerId)
        {
            return start.UtcTime.CompareTo(end.UtcTime);
        }

        if (start.HardwareTime.HasValue && end.HardwareTime.HasValue)
        {
            return start.HardwareTime.Value.CompareTo(end.HardwareTime.Value);
        }

        return start.SoftwareTime.CompareTo(end.SoftwareTime);
    }
}

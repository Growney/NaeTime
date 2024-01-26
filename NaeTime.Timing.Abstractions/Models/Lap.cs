namespace NaeTime.Timing.Abstractions.Models;
public class Lap
{
    public Pass Start { get; }
    public Pass End { get; }

    public Lap(Pass start, Pass end)
    {
        Start = start;
        End = end;
    }
}

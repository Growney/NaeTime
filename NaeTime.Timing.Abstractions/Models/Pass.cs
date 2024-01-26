namespace NaeTime.Timing.Abstractions.Models;
public class Pass
{
    public Guid Id { get; }
    public Detection Detection { get; }
    public Guid PilotId { get; }
    public Guid FlyingSessionId { get; }
    public Guid TrackId { get; }
    public int TimedGatePosition { get; }
}

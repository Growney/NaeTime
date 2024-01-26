namespace NaeTime.Timing.Abstractions.Models;
public class Pass
{
    public Pass(Guid id, Detection detection, Guid pilotId, Guid flyingSessionId, Guid trackId, int timedGatePosition)
    {
        Id = id;
        Detection = detection ?? throw new ArgumentNullException(nameof(detection));
        PilotId = pilotId;
        FlyingSessionId = flyingSessionId;
        TrackId = trackId;
        TimedGatePosition = timedGatePosition;
    }

    public Guid Id { get; }
    public Detection Detection { get; }
    public Guid PilotId { get; }
    public Guid FlyingSessionId { get; }
    public Guid TrackId { get; }
    public int TimedGatePosition { get; }
}

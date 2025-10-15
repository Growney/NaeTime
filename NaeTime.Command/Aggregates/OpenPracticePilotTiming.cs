using EventDbLite.Aggregates;
using NaeTime.Events;
using static NaeTime.Command.Aggregates.OpenPracticePilotTiming;

namespace NaeTime.Command.Aggregates;
public class OpenPracticePilotTiming : AggregateRoot<OpenPracticeTimingId>
{
    public class OpenPracticeTimingId
    {
        public Guid PilotId { get; init; }
        public Guid SessionId { get; init; }

        public override string ToString() => $"{SessionId:N}-{PilotId:N}";
    }
    private class Detection
    {
        public Guid Id { get; init; }
        public byte OrdinalPosition { get; init; }
        public ulong? HardwareTime { get; init; }
        public long SoftwareTime { get; init; }
        public DateTime UtcTime { get; init; }
        public bool IsValid { get; set; } = true;
    }

    private class Lap
    {
        public uint Id { get; init; }
        public required Detection StartDetection { get; set; }
        public Detection? EndDetection { get; set; }
        public bool IsIncluded { get; set; } = true;
        public List<Split> Splits { get; } = [];
    }
    private class Split
    {
        public uint Id { get; init; }
        public required Detection StartDetection { get; set; }
        public Detection? EndDetection { get; set; }
    }

    private readonly List<Lap> _laps = [];

    private Lap? _currentLap = null;


    public OpenPracticePilotTiming()
    {

    }

    public OpenPracticePilotTiming(Guid sessionId, Guid pilotId)
    {
        Raise(new PilotOpenPracticeSessionTimingStarted(pilotId, sessionId));
    }
    private void When(PilotOpenPracticeSessionTimingStarted e)
    {
        Id = new OpenPracticeTimingId
        {
            PilotId = e.PilotId,
            SessionId = e.SessionId
        };
    }

    public void AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, byte ordinalPosition, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Raise(new OpenPracticeDetectionAddedToPilot(pilotId, sessionId, detectionId, ordinalPosition, hardwareTime, softwareTime, utcTime));

        if (ordinalPosition == 0)
        {

        }

    }

}

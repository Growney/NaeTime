namespace NaeTime.Command.Abstractions;
public interface IOpenPracticePilotTimingCommandHandler
{
    public Task AddDetectionToPilot(Guid pilotId, Guid sessionId, Guid detectionId, byte ordinalPosition, byte detectorCount, ulong? hardwareTime, long softwareTime, DateTime utcTime);
}

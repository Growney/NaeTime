namespace NaeTime.Command.Abstractions;
public interface ITrackCommandHandler
{
    public Task DesignTrack(Guid id, string name, Guid[] detectors);
    public Task RenameTrack(Guid id, string name);
    public Task ReorderTrackDetectors(Guid trackId, Guid[] detectors);
    public Task SetMaximumLapTime(Guid trackId, long milliseconds);
    public Task SetMinimumLapTime(Guid trackId, long milliseconds);
    public Task ResetMaximumLapTime(Guid trackId);
    public Task ResetMinimumLapTime(Guid trackId);
    public Task SetPilotMaximumLapTime(Guid trackId, Guid pilotId, long milliseconds);
    public Task SetPilotMinimumLapTime(Guid trackId, Guid pilotId, long milliseconds);
    public Task ResetPilotMaximumLapTime(Guid trackId, Guid pilotId);
    public Task ResetPilotMinimumLapTime(Guid trackId, Guid pilotId);
}

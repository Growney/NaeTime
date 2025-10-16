namespace NaeTime.Command.Abstractions;
public interface ITrackCommandHandler
{
    public Task DesignTrack(Guid id, string name, Guid[] detectors);
    public Task RenameTrack(Guid id, string name);
    public Task ReorderTrackDetectors(Guid trackId, Guid[] detectors);
    public Task SetMaximumLapTime(Guid trackId, long milliseconds);
    public Task SetMinimumDetectionDelay(Guid trackId, long milliseconds);
    public Task ResetMaximumLapTime(Guid trackId);
    public Task ResetMinimumDetectionDelay(Guid trackId);
}

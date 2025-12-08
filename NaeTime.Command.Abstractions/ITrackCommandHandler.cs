namespace NaeTime.Command.Abstractions;
public interface ITrackCommandHandler
{
    public Task DesignTrack(Guid id, string name, Guid[] detectors);
    public Task RenameTrack(Guid id, string name);
    public Task ReorderTrackDetectors(Guid trackId, Guid[] detectors);
}

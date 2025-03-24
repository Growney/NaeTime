namespace NaeTime.Persistence.Abstractions.Management;

public interface IManagementRepository
{
    public Task<IEnumerable<Pilot>> GetPilots();
    public Task<Pilot?> GetPilot(Guid pilotId);
    public Task<Track?> GetTrack(Guid trackId);
    public Task<IEnumerable<Track>> GetTracks();
    public Task<ActiveSession?> GetActiveSession();
}

namespace NaeTime.Persistence.Abstractions;
public interface IRepositoryFactory
{
    Task<IHardwareRepository> CreateHardwareRepository();
    Task<IPilotRepository> CreatePilotRepository();
    Task<ITimingRepository> CreateTimingRepository();
    Task<ITrackRepository> CreateTrackRepository();
    Task<IActiveRepository> CreateActiveRepository();
}

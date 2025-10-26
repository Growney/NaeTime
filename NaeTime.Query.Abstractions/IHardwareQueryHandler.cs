using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface IHardwareQueryHandler
{
    public Task<Detector?> GetDetector(Guid id);
    public Task<IEnumerable<Detector>> GetAllDetectors();
    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids);
    public Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id);
    public Task<IEnumerable<ImmersionRCLapRF>> GetAllImmersionRCLapRFs();
    public Task<IEnumerable<DesiredImmersionRCLapRFLane>> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId);
    public Task<IEnumerable<TimerDetails>> GetDetails(Guid timerId);
    public Task<IEnumerable<TimerDetails>> GetLaneDetails(Guid timerId, byte laneId);
    //public Task<SerialNaeTimeNode?> GetSerialNaeTimeNode(Guid id);
}

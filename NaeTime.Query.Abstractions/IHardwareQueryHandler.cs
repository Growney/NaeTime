using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface IHardwareQueryHandler
{
    public Task<Detector?> GetDetector(Guid id);
    public Task<IEnumerable<Detector>> GetAllDetectors();
    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids);

    public Task<TimerDetails> GetDetails(Guid timerId);
    public Task<TimerLaneDetails> GetLaneDetails(Guid timerId, byte laneId);

    public Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id);
    public Task<ImmersionRCLapRFLane?> GetImmersionRCLapRFLane(Guid timerId, byte laneId);
    public Task<IEnumerable<ImmersionRCLapRF>> GetAllImmersionRCLapRFs();
    public Task<IEnumerable<DesiredImmersionRCLapRFLane>> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId);

    public Task<NetworkNaeTimeNode?> GetNetworkNaeTimeNode(Guid id);
    public Task<NaeTimeNodeLane?> GetNaeTimeNodeLane(Guid timerId, byte laneId);
    public Task<IEnumerable<NaeTimeNode>> GetAllNaeTimeNodes();
    public Task<IEnumerable<DesiredNaeTimeNodeLane>> GetActiveNaeTimeNodeLanesConfiguration(Guid timerId);
}

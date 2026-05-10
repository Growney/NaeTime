using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;

public class HardwareQueryHandler : IHardwareQueryHandler
{
    private readonly IDetectorProjection _detectorProjection;
    private readonly IImmersionRCProjection _immersionRCProjection;
    private readonly INaeTimeNodeProjection _naeTimeNodeProjection;
    private readonly ITimerConfigurationProjection _timerConfigurationProjection;
    private readonly ITimerDetailsProjection _timerDetailsProjection;
    private readonly IInterfaceProjection _interfaceProjection;
    private readonly IELRSBackpackProjection _elrsBackpackProjection;

    public HardwareQueryHandler(IDetectorProjection detectorProjection, IImmersionRCProjection immersionRCProjection, INaeTimeNodeProjection naeTimeNodeProjection, ITimerConfigurationProjection timerConfigurationProjection, ITimerDetailsProjection timerDetailsProjection, IInterfaceProjection interfaceProjection, IELRSBackpackProjection elrsBackpackProjection)
    {
        _detectorProjection = detectorProjection ?? throw new ArgumentNullException(nameof(detectorProjection));
        _immersionRCProjection = immersionRCProjection ?? throw new ArgumentNullException(nameof(immersionRCProjection));
        _naeTimeNodeProjection = naeTimeNodeProjection ?? throw new ArgumentNullException(nameof(naeTimeNodeProjection));
        _timerConfigurationProjection = timerConfigurationProjection;
        _timerDetailsProjection = timerDetailsProjection;
        _interfaceProjection = interfaceProjection ?? throw new ArgumentNullException(nameof(interfaceProjection));
        _elrsBackpackProjection = elrsBackpackProjection ?? throw new ArgumentNullException(nameof(elrsBackpackProjection));
    }

    public Task<Detector?> GetDetector(Guid id) => Task.FromResult(_detectorProjection.GetDetector(id));

    public Task<IEnumerable<Detector>> GetAllDetectors() => Task.FromResult(_detectorProjection.GetDetectors());

    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids) => Task.FromResult(_detectorProjection.GetDetectors(ids));

    public Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id) => Task.FromResult(_immersionRCProjection.GetImmersionRCLapRF(id));

    public Task<ImmersionRCLapRFLane?> GetImmersionRCLapRFLane(Guid timerId, byte laneId) => Task.FromResult(_immersionRCProjection.GetImmersionRCLapRFLane(timerId, laneId));
    public Task<IEnumerable<ImmersionRCLapRF>> GetAllImmersionRCLapRFs() => Task.FromResult(_immersionRCProjection.GetAllImmersionRCLapRFs());
    public Task<IEnumerable<DesiredImmersionRCLapRFLane>> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId) => Task.FromResult(_timerConfigurationProjection.GetActiveImmersionRCLapRFLanesConfiguration(timerId));

    public Task<TimerDetails> GetDetails(Guid timerId) => Task.FromResult(_timerDetailsProjection.GetDetails(timerId));
    public Task<TimerLaneDetails> GetLaneDetails(Guid timerId, byte laneId) => Task.FromResult(_timerDetailsProjection.GetLaneDetails(timerId, laneId));

    public Task<NetworkNaeTimeNode?> GetNetworkNaeTimeNode(Guid id) => Task.FromResult(_naeTimeNodeProjection.GetNetworkNaeTimeNode(id));

    public Task<NaeTimeNodeLane?> GetNaeTimeNodeLane(Guid timerId, byte laneId) => Task.FromResult(_naeTimeNodeProjection.GetNaeTimeNodeLane(timerId, laneId));

    public Task<IEnumerable<NaeTimeNode>> GetAllNaeTimeNodes() => Task.FromResult(_naeTimeNodeProjection.GetAllNaeTimeNodes());

    public Task<IEnumerable<DesiredNaeTimeNodeLane>> GetActiveNaeTimeNodeLanesConfiguration(Guid timerId) => Task.FromResult(_timerConfigurationProjection.GetActiveNaeTimeNodeLanesConfiguration(timerId));

    public Task<Interface?> GetInterface(Guid id) => Task.FromResult(_interfaceProjection.GetInterface(id));

    public Task<IEnumerable<Interface>> GetAllInterfaces() => Task.FromResult(_interfaceProjection.GetInterfaces());

    public Task<IEnumerable<Interface>> GetInterfaces(IEnumerable<Guid> ids) => Task.FromResult(_interfaceProjection.GetInterfaces(ids));

    public Task<SerialELRSBackpackInterface?> GetSerialELRSBackpackInterface(Guid id) => Task.FromResult(_elrsBackpackProjection.GetSerialELRSBackpackInterface(id));

    public Task<IEnumerable<SerialELRSBackpackInterface>> GetAllSerialELRSBackpackInterfaces() => Task.FromResult(_elrsBackpackProjection.GetAllSerialELRSBackpackInterfaces());
}
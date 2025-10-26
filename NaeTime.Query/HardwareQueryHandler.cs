using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections.Abstractions;

namespace NaeTime.Query;

public class HardwareQueryHandler : IHardwareQueryHandler
{
    private readonly IDetectorProjection _detectorProjection;
    private readonly IImmersionRCProjection _immersionRCProjection;
    private readonly ITimerConfigurationProjection _timerConfigurationProjection;

    public HardwareQueryHandler(IDetectorProjection detectorProjection, IImmersionRCProjection immersionRCProjection, ITimerConfigurationProjection timerConfigurationProjection)
    {
        _detectorProjection = detectorProjection ?? throw new ArgumentNullException(nameof(detectorProjection));
        _immersionRCProjection = immersionRCProjection ?? throw new ArgumentNullException(nameof(immersionRCProjection));
        _timerConfigurationProjection = timerConfigurationProjection;
    }

    public Task<Detector?> GetDetector(Guid id) => Task.FromResult(_detectorProjection.GetDetector(id));

    public Task<IEnumerable<Detector>> GetAllDetectors() => Task.FromResult(_detectorProjection.GetDetectors());

    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids) => Task.FromResult(_detectorProjection.GetDetectors(ids));

    public Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id) => Task.FromResult(_immersionRCProjection.GetImmersionRCLapRF(id));

    public Task<IEnumerable<ImmersionRCLapRF>> GetAllImmersionRCLapRFs() => Task.FromResult(_immersionRCProjection.GetAllImmersionRCLapRFs());
    public Task<IEnumerable<DesiredImmersionRCLapRFLane>> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId) => Task.FromResult(_timerConfigurationProjection.GetActiveImmersionRCLapRFLanesConfiguration(timerId));
}
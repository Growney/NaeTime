using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections.Abstractions;

namespace NaeTime.Query;

public class HardwareQueryHandler : IHardwareQueryHandler
{
    private readonly IDetectorProjection _detectorProjection;
    private readonly IImmersionRCProjection _immersionRCProjection;

    public HardwareQueryHandler(IDetectorProjection detectorProjection, IImmersionRCProjection immersionRCProjection)
    {
        _detectorProjection = detectorProjection ?? throw new ArgumentNullException(nameof(detectorProjection));
        _immersionRCProjection = immersionRCProjection ?? throw new ArgumentNullException(nameof(immersionRCProjection));
    }

    public Task<Detector?> GetDetector(Guid id) => Task.FromResult(_detectorProjection.GetDetector(id));

    public Task<IEnumerable<Detector>> GetAllDetectors() => Task.FromResult(_detectorProjection.GetDetectors());

    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids) => Task.FromResult(_detectorProjection.GetDetectors(ids));

    public Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id) => Task.FromResult(_immersionRCProjection.GetImmersionRCLapRF(id));

    public Task<SerialNaeTimeNode?> GetSerialNaeTimeNode(Guid id)
    {
        throw new NotImplementedException();
    }
}
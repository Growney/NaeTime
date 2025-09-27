using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class HardwareQueryHandler : IHardwareQueryHandler
{
    private readonly IProjectionProvider _projectionProvider;

    public HardwareQueryHandler(IProjectionProvider projectionProvider)
    {
        _projectionProvider = projectionProvider;
    }

    public async Task<Detector?> GetDetector(Guid id)
    {
        DetectorList detectors = await _projectionProvider.Load<DetectorList>();
        return detectors.GetDetector(id);
    }

    public async Task<IEnumerable<Detector>> GetAllDetectors()
    {
        DetectorList detectors = await _projectionProvider.Load<DetectorList>();
        return detectors.GetDetectors();
    }

    public async Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids)
    {
        DetectorList detectors = await _projectionProvider.Load<DetectorList>();
        return detectors.GetDetectors(ids);
    }

    public Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<SerialNaeTimeNode?> GetSerialNaeTimeNode(Guid id)
    {
        throw new NotImplementedException();
    }
}
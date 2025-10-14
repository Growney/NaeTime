using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class HardwareQueryHandler(IProjectionProvider projectionProvider) : IHardwareQueryHandler
{
    private readonly IProjectionProvider _projectionProvider = projectionProvider;

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

    public async Task<Ethernet8ChannelImmersionRCLapRF?> GetEthernet8ChannelImmersionRCLapRF(Guid id)
    {
        ImmersionRCList immersionRCList = await _projectionProvider.Load<ImmersionRCList>();
        return immersionRCList.GetImmersionRCLapRF(id);
    }

    public Task<SerialNaeTimeNode?> GetSerialNaeTimeNode(Guid id)
    {
        throw new NotImplementedException();
    }
}
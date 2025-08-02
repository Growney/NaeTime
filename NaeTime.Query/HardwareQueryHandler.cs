using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class HardwareQueryHandler : IHardwareQueryHandler
{
    private readonly DetectorList _detectorList;

    public HardwareQueryHandler(DetectorList detectorList)
    {
        _detectorList = detectorList;
    }

    public Task<Detector?> GetDetector(Guid id)
        => Task.FromResult(_detectorList.GetDetector(id));

    public Task<IEnumerable<Detector>> GetAllDetectors()
        => Task.FromResult(_detectorList.GetAllDetectors());

    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids)
         => Task.FromResult(_detectorList.GetDetectors(ids));
}
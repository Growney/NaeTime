using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IDetectorProjection
{
    Detector? GetDetector(Guid id);
    IEnumerable<Detector> GetDetectors();
    IEnumerable<Detector> GetDetectors(IEnumerable<Guid> ids);
}
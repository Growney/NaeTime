using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface IDetectorProjection
{
    Detector? GetDetector(Guid id);
    IEnumerable<Detector> GetDetectors();
    IEnumerable<Detector> GetDetectors(IEnumerable<Guid> ids);
}
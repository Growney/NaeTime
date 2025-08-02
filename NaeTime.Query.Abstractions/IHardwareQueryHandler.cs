using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface IHardwareQueryHandler
{
    public Task<Detector?> GetDetector(Guid id);
    public Task<IEnumerable<Detector>> GetAllDetectors();
    public Task<IEnumerable<Detector>> GetDetectors(IEnumerable<Guid> ids);
}

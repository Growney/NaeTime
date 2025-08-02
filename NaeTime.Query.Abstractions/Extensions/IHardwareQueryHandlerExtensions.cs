using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Extensions;
public static class IHardwareQueryHandlerExtensions
{
    public static async Task<Detector[]> GetDetectorsInOrder(this IHardwareQueryHandler handler, IEnumerable<Guid> ids)
    {
        IEnumerable<Detector> detectors = await handler.GetDetectors(ids);

        Dictionary<Guid, Detector> indexedDetectors = detectors.ToDictionary(d => d.Id);

        Detector[] orderedDetectors = new Detector[indexedDetectors.Count()];

        for (int i = 0; i < orderedDetectors.Length; i++)
        {
            if (indexedDetectors.TryGetValue(ids.ElementAt(i), out var detector))
            {
                orderedDetectors[i] = detector;
            }
            else
            {
                throw new KeyNotFoundException($"Detector with ID {ids.ElementAt(i)} not found in the provided detectors.");
            }
        }

        return orderedDetectors;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client;
public static class DetectorProjector
{
    [return: NotNullIfNotNull(nameof(source))]
    public static Models.Detector? Project(Query.Abstractions.Models.Detector? source)
    {
        if(source == null)
        {
            return null;
        }
        return new()
        {
            Id = source.Id,
            Name = source.Name,
            SupportedLanes = source.SupportedLanes,
            Type = source.Type switch
            {
                Query.Abstractions.Models.DetectorType.EthernetLapRF8Channel => Models.DetectorType.EthernetLapRF8Channel,
                Query.Abstractions.Models.DetectorType.NaeTimeSerial => Models.DetectorType.NaeTimeSerial,
                _ => throw new NotSupportedException($"Detector type {source.Type} is not supported.")
            }
        };
    }
    public static IEnumerable<Models.Detector> Project(IEnumerable<Query.Abstractions.Models.Detector> source)
    {
        foreach(Query.Abstractions.Models.Detector detector in source)
        {
            if(detector == null)
            {
                continue;
            }

            yield return Project(detector);
        }
    }
}

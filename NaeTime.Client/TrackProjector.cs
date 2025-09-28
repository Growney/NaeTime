using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client;
public static class TrackProjector
{
    public static Models.Track? Project(NaeTime.Query.Abstractions.Models.Track? track)
    {
        if(track == null)
        {
            return null;
        }

        return new Models.Track
        {
            Id = track.Id,
            Name = track.Name,
            Detectors = track.Detectors.Select(d => new Models.Detector
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type switch
                {
                    NaeTime.Query.Abstractions.Models.DetectorType.EthernetLapRF8Channel => Models.DetectorType.EthernetLapRF8Channel,
                    NaeTime.Query.Abstractions.Models.DetectorType.NaeTimeSerial => Models.DetectorType.NaeTimeSerial,
                    _ => throw new NotImplementedException()
                },
                SupportedLanes = d.SupportedLanes
            }).ToList(),
            MaxLanes = track.MaxLanes,
            MinimumLapTimeMilliseconds = track.MinimumLapTimeMilliseconds,
            MaximumLapTimeMilliseconds = track.MaximumLapTimeMilliseconds
        };
    }
}

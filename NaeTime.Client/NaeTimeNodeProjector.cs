using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NaeTime.Client;

public static class NaeTimeNodeProjector
{
    [return: NotNullIfNotNull(nameof(source))]
    public static Models.EthernetNaeTimeNode? Project(Query.Abstractions.Models.NetworkNaeTimeNode? source)
    {
        if (source == null)
        {
            return null;
        }

        return new Models.EthernetNaeTimeNode
        {
            Id = source.Id,
            Name = source.Name,
            Lanes = source.Lanes.Select(lane => new Models.NaeTimeNodeLane
            {
                LaneId = lane.LaneId,
                IsEnabled = lane.IsEnabled.Requested ?? false,
                EntryThreshold = lane.EntryThreshold.Requested ?? 0,
                ExitThreshold = lane.ExitThreshold.Requested ?? 0,
                BandId = lane.BandId.Requested,
                FrequencyInMHz = lane.FrequencyInMHz.Requested ?? 0,
            }).ToList(),
            IPAddress = source.IPAddress.ToString(),
            Port = source.Port
        };
    }
}

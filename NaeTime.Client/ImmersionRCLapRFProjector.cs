using System.Diagnostics.CodeAnalysis;

namespace NaeTime.Client;
public class ImmersionRCLapRFProjector
{
    [return: NotNullIfNotNull(nameof(source))]
    public static Models.EthernetImmersionRCLapRF? Project(Query.Abstractions.Models.Ethernet8ChannelImmersionRCLapRF? source)
    {
        if (source == null)
        {
            return null;
        }

        return new Models.EthernetImmersionRCLapRF
        {
            Id = source.Id,
            Name = source.Name,
            Lanes = source.Lanes.Select(lane => new Models.ImmersionRCLapRFLane
            {
                LaneId = lane.LaneId,
                IsEnabled = lane.IsEnabled.Requested,
                Gain = lane.Gain.Requested ?? 0,
                Threshold = lane.Threshold.Requested ?? 0,
                BandId = lane.BandId.Requested,
                FrequencyInMHz = lane.FrequencyInMHz.Requested,
            }).ToList(),
            IPAddress = source.IPAddress.ToString(),
            Port = source.Port
        };
    }
}

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
                IsEnabled = lane.IsEnabled,
                Gain = lane.Gain,
                Threshold = lane.Threshold,
                BandId = lane.BandId,
                FrequencyInMHz = lane.FrequencyInMHz,
            }).ToList(),
            IPAddress = source.IPAddress.ToString(),
            Port = source.Port
        };
    }
}

using System.Net;

namespace NaeTime.Messages.Responses;
public record EthernetLapRF8ChannelTimersResponse(IEnumerable<EthernetLapRF8ChannelTimersResponse.EthernetLapRF8Channel> Timers)
{
    public record class EthernetLapRF8Channel(Guid TimerId, IPAddress IpAddress, int Port);
}
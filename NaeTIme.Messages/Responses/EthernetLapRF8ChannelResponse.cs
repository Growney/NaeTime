using System.Net;

namespace NaeTime.Messages.Responses;
public record EthernetLapRF8ChannelResponse(Guid Id, String Name, IPAddress IpAddress, int Port);


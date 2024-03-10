using System.Net;

namespace NaeTime.Hardware.Messages.Responses;
public record EthernetLapRF8ChannelResponse(Guid Id, string Name, IPAddress IpAddress, int Port);


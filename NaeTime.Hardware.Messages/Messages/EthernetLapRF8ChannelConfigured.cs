using System.Net;

namespace NaeTime.Hardware.Messages.Messages;
public record EthernetLapRF8ChannelConfigured(Guid TimerId, string Name, IPAddress IpAddress, int Port);
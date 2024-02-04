using System.Net;

namespace NaeTime.Messages.Events.Hardware;
public record EthernetLapRF8ChannelConfigured(Guid TimerId, String Name, IPAddress IpAddress, int Port);
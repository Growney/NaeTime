using System.Net;

namespace NaeTime.Persistence.Abstractions.Hardware;
public record class EthernetLapRF8ChannelTimer(Guid TimerId, string Name, IPAddress IpAddress, int Port);
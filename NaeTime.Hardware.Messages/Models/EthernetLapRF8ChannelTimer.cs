using System.Net;

namespace NaeTime.Hardware.Messages.Models;
public record class EthernetLapRF8ChannelTimer(Guid TimerId, string Name, IPAddress IpAddress, int Port);
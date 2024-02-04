using System.Net;

namespace NaeTime.Messages.Models;
public record EthernetLapRF8Channel(Guid TimerId, IPAddress IpAddress, int Port);

using System.Net;

namespace NaeTime.Timing.Abstractions.Models;
public record EthernetLapRF8Channel(Guid TimerId, IPAddress Address, int Port);

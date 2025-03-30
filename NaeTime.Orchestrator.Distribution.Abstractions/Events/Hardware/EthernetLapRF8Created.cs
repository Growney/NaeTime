using System.Net;

namespace NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;

public record EthernetLapRF8Created(Guid Id, string Name, IPAddress IpAddress, int Port);

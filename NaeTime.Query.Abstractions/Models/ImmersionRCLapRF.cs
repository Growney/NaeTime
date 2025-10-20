namespace NaeTime.Query.Abstractions.Models;
public record ImmersionRCLapRF(Guid Id, string? Name, bool IsConnected, ImmersionRCLapRFLane[] Lanes);
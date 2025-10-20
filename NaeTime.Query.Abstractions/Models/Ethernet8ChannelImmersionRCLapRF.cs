using System.Net;

namespace NaeTime.Query.Abstractions.Models;
public record Ethernet8ChannelImmersionRCLapRF(Guid Id, string Name, bool IsConnected, ImmersionRCLapRFLane[] Lanes, IPAddress IPAddress, ushort Port) : ImmersionRCLapRF(Id, Name, IsConnected, Lanes);

using System.Net;

namespace NaeTime.Query.Abstractions.Models;
public record Ethernet8ChannelImmersionRCLapRF(Guid Id, string Name, ImmersionRCLapRFLane[] Lanes, IPAddress IPAddress, ushort Port) : ImmersionRCLapRF(Id, Name, Lanes);

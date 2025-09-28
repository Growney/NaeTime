using System.Net;

namespace NaeTime.Query.Abstractions.Models;
public record Ethernet8ChannelImmersionRCLapRF(Guid Id, string Name, bool IsConnected, bool IsSetupConfirmed, ImmersionRCLapRFLane[] RequestedLanes, ImmersionRCLapRFLane[] ConfirmedLanes, IPAddress IPAddress, ushort Port) : ImmersionRCLapRF(Id, Name, IsConnected, IsSetupConfirmed, RequestedLanes, ConfirmedLanes);

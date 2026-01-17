using System.Net;

namespace NaeTime.Query.Abstractions.Models;
public record NetworkNaeTimeNode(Guid Id, string Name, bool IsConnected, NaeTimeNodeLane[] Lanes, string IPAddress, ushort Port) : NaeTimeNode(Id, Name, Lanes);

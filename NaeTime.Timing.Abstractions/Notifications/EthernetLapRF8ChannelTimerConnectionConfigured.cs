using System.Net;

namespace NaeTime.Timing.Abstractions.Notifications;
public record EthernetLapRF8ChannelTimerConnectionConfigured(Guid TimerId, IPAddress IpAddress, int Port);


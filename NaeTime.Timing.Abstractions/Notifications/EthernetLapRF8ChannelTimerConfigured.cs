using System.Net;

namespace NaeTime.Timing.Abstractions.Notifications;
public record EthernetLapRF8ChannelTimerConfigured(Guid TimerId, IPAddress IpAddress, int Port);


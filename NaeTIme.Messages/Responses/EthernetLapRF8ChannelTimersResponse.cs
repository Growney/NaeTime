using NaeTime.Messages.Models;

namespace NaeTime.Messages.Responses;
public record EthernetLapRF8ChannelTimersResponse(IEnumerable<EthernetLapRF8Channel> Timers);
using NaeTime.Messages.Models;

namespace NaeTime.Messages.Responses;
public record ActiveTimingsResponse(Guid SessionId, byte Lane, ActiveLap? Lap, ActiveSplit? Split);
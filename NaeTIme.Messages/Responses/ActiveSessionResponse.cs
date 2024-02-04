using NaeTime.Timing.Messages.Models;

namespace NaeTime.Messages.Responses;
public record ActiveSessionResponse(Guid SessionId, SessionType SessionType);

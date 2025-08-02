namespace NaeTime.Query.Abstractions.Models;
public record Pilot(Guid Id, string? Firstname, string? Lastname, string? Callsign, bool HasBindingPhrase);

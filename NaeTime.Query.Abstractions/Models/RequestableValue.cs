namespace NaeTime.Query.Abstractions.Models;
public record RequestableValue<T>(T? Requested, T? Confirmed, bool ConfirmedMismatch)
{
    public bool IsMismatch => Requested?.Equals(Confirmed) ?? false;
}
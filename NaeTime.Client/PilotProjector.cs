using System.Diagnostics.CodeAnalysis;

namespace NaeTime.Client;
public static class PilotProjector
{
    [return: NotNullIfNotNull(nameof(source))]
    public static Models.Pilot? Project(Query.Abstractions.Models.Pilot? source)
    {
        if (source == null)
        {
            return null;
        }

        return new()
        {
            Id = source.Id,
            Firstname = source.Firstname,
            Lastname = source.Lastname,
            Callsign = source.Callsign,
            HasBindingPhrase = source.HasBindingPhrase
        };
    }
}

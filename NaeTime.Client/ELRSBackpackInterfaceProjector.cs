using System.Diagnostics.CodeAnalysis;

namespace NaeTime.Client;

public class ELRSBackpackInterfaceProjector
{
    [return: NotNullIfNotNull(nameof(source))]
    public static Models.SerialELRSBackpackInterface? Project(Query.Abstractions.Models.SerialELRSBackpackInterface? source)
    {
        if (source == null)
        {
            return null;
        }

        return new Models.SerialELRSBackpackInterface
        {
            Id = source.Id,
            Name = source.Name,
            ComPort = source.ComPort,
        };
    }
}

using NodaTime;
using System;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public struct CommandMetadata
    {
        public CommandMetadata(string commandIdentifier, ZonedDateTime createdAt, ZonedDateTime validFrom)
        {
            CreatedAt = createdAt;
            ValidFrom = validFrom;
            CommandIdentifier = commandIdentifier ?? throw new ArgumentNullException(nameof(commandIdentifier));
        }

        public ZonedDateTime CreatedAt { get; }
        public ZonedDateTime ValidFrom { get; }
        public string CommandIdentifier { get; }

    }
}

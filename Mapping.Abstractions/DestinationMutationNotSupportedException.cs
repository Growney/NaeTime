using System.Runtime.Serialization;

namespace Mapping.Abstractions;

public class DestinationMutationNotSupportedException : Exception
{
    public DestinationMutationNotSupportedException()
    {
    }

    public DestinationMutationNotSupportedException(string? message) : base(message)
    {
    }

    public DestinationMutationNotSupportedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected DestinationMutationNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

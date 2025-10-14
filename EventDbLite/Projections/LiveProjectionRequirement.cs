namespace EventDbLite.Projections;

public class LiveProjectionRequirement(string? stream, Type projectionType)
{
    public string? Stream { get; } = stream;
    public Type ProjectionType { get; } = projectionType ?? throw new ArgumentNullException(nameof(projectionType));
}

namespace EventDbLite.Projections;

public class LiveProjectionRequirement
{
    public LiveProjectionRequirement(string? stream, Type projectionType)
    {
        Stream = stream;
        ProjectionType = projectionType ?? throw new ArgumentNullException(nameof(projectionType));
    }

    public string? Stream { get; } = null;
    public Type ProjectionType { get; }
}

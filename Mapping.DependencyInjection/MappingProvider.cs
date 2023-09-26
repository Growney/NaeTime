namespace Mapping.Abstractions;

internal class MappingProvider
{
    public MappingProvider(Type source, Type destination, Func<object, object?, object> map)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        Map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public Type Source { get; }
    public Type Destination { get; }
    public Func<object, object?, object> Map { get; }
}

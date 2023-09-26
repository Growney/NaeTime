using Mapping.Abstractions;

namespace Mapping.DependencyInjection;

internal class DIMappingBuilder : IMappingBuilder
{

    private readonly Dictionary<Type, Dictionary<Type, Func<object, object?, object>>> _mappings = new();

    public DIMappingBuilder(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IServiceProvider ServiceProvider { get; }

    public void AddMapping(Type source, Type destination, Func<object, object?, object> mapping)
    {
        if (!_mappings.TryGetValue(source, out var sourceDictionary))
        {
            sourceDictionary = new Dictionary<Type, Func<object, object?, object>>();
            _mappings.Add(source, sourceDictionary);
        }

        if (!sourceDictionary.ContainsKey(destination))
        {
            sourceDictionary.Add(destination, mapping);
        }

    }

    internal IEnumerable<(Type source, Type destination, Func<object, object?, object> mapping)> GetMappings()
    {
        foreach (var sourceDirectionary in _mappings)
        {
            foreach (var mapping in sourceDirectionary.Value)
            {
                yield return (sourceDirectionary.Key, mapping.Key, mapping.Value);
            }
        }
    }

}

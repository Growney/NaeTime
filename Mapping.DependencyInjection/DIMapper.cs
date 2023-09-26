using Mapping.Abstractions;

namespace Mapping.DependencyInjection;

internal class DIMapper : IMapper
{
    private readonly Dictionary<Type, Dictionary<Type, Func<object, object?, object>>> _mappings;

    public DIMapper(DIMappingBuilder mappings)
    {
        _mappings = IndexMappings(mappings);
    }

    private static Dictionary<Type, Dictionary<Type, Func<object, object?, object>>> IndexMappings(DIMappingBuilder mappings)
    {
        var indexedMappings = new Dictionary<Type, Dictionary<Type, Func<object, object?, object>>>();
        foreach (var mapping in mappings.GetMappings())
        {
            if (!indexedMappings.TryGetValue(mapping.source, out var sourceDictionary))
            {
                sourceDictionary = new Dictionary<Type, Func<object, object?, object>>();
                indexedMappings.Add(mapping.source, sourceDictionary);
            }

            if (!sourceDictionary.ContainsKey(mapping.destination))
            {
                sourceDictionary.Add(mapping.destination, mapping.mapping);
            }
            else
            {
                throw new InvalidOperationException($"{mapping.source} already has a mapping to type {mapping.destination}");
            }
        }
        return indexedMappings;
    }

    public Func<object, object?, object> Get(Type sourceType, Type destinationType)
    {
        if (!_mappings.TryGetValue(sourceType, out var sourceDirectionary))
        {
            throw new InvalidOperationException($"{sourceType} does not have any mappings");
        }

        if (!sourceDirectionary.TryGetValue(destinationType, out var mapping))
        {
            throw new InvalidOperationException($"{sourceType} does not have a mapping to {destinationType}");
        }

        return mapping;
    }
}

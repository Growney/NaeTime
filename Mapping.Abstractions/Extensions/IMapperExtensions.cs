using Mapping.Abstractions;

namespace Tensor.Mapping.Abstractions;

public static class IMapperExtensions
{
    public static TDestination? MapOrDefault<TDestination>(this IMapper mapper, object? source)
    {
        if (source == null)
        {
            return default;
        }

        var sourceType = source.GetType();
        var mapping = mapper.Get(sourceType, typeof(TDestination));
        var mapped = mapping(source, null);
        return (TDestination)mapped;
    }
    public static TDestination? MapOrDefault<TSource, TDestination>(this IMapper mapper, TSource? source)
    {
        if (source == null)
        {
            return default;
        }

        var mapping = mapper.Get(typeof(TSource), typeof(TDestination));
        var mapped = mapping(source, null);
        return (TDestination)mapped;
    }
    public static TDestination? MapOrDefault<TSource, TDestination>(this IMapper mapper, TSource? source, TDestination? destination)
    {
        if (source == null)
        {
            return default;
        }

        var mapping = mapper.Get(typeof(TSource), typeof(TDestination));
        var mapped = mapping(source, destination);
        return (TDestination)mapped;
    }

    public static TDestination Map<TDestination>(this IMapper mapper, object source)
    {
        var sourceType = source.GetType();
        var mapping = mapper.Get(sourceType, typeof(TDestination));
        var mapped = mapping(source, null);
        return (TDestination)mapped;
    }
    public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source)
        where TSource : notnull
    {
        var mapping = mapper.Get(typeof(TSource), typeof(TDestination));
        var mapped = mapping(source, null);
        return (TDestination)mapped;
    }
    public static TDestination Map<TSource, TDestination>(this IMapper mapper, TSource source, TDestination? destination)
        where TSource : notnull
    {
        var mapping = mapper.Get(typeof(TSource), typeof(TDestination));
        var mapped = mapping(source, destination);
        return (TDestination)mapped;
    }
    public static IEnumerable<TDestination> Map<TSource, TDestination>(this IMapper mapper, IEnumerable<TSource> source)
        where TSource : notnull
        where TDestination : notnull
    {
        var mapping = mapper.Get(typeof(TSource), typeof(TDestination));

        foreach (var sourceItem in source)
        {
            var mapped = mapping(sourceItem, null);
            yield return (TDestination)mapped;
        }
    }
}

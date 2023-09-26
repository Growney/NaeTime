
namespace Mapping.Abstractions;

public interface IUnidirectionalMapper<TSource, TDestination>
    where TDestination : notnull
{
    TDestination Map(TSource source, TDestination? destination);
}

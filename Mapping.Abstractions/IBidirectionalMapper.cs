using Tensor.Mapping.Abstractions;

namespace Mapping.Abstractions;

public interface IBidirectionalMapper<T, K> : IUnidirectionalMapper<T, K>
    where T : notnull
    where K : notnull
{
    T Map(K source, T? destination);
}

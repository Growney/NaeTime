namespace Mapping.Abstractions;

public interface IMapper
{
    Func<object, object?, object> Get(Type sourceType, Type destinationType);
}

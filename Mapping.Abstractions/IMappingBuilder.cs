namespace Mapping.Abstractions;

public interface IMappingBuilder
{
    IServiceProvider ServiceProvider { get; }
    void AddMapping(Type source, Type destination, Func<object, object?, object> mapping);
}

namespace NaeTime.PubSub.Abstractions;
public interface IEventRegistrar
{
    IEventRegistrarScope CreateScope();
    void RegisterHandler(Type type, Func<object, Task> handler);
}

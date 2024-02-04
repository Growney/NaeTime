namespace NaeTime.PubSub.Abstractions;
public interface IDispatcher
{
    Task Dispatch<T>(T message) where T : class;
    Task<TResponse?> Request<TRequest, TResponse>(TRequest request) where TRequest : notnull;
}

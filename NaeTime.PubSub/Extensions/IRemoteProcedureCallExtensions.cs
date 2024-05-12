namespace NaeTime.PubSub.Abstractions;
public static class IRemoteProcedureCallExtensions
{
    public static async Task<TResult?> InvokeAsync<TResult>(this IRemoteProcedureCallClient manager, string methodName, params object[] parameters)
    {
        RPCSignature signature = new(methodName, typeof(TResult), parameters.Select(x => x.GetType()).ToArray());

        Func<object?[], Task<object?>> handler = manager.GetHandler(signature) ?? throw new InvalidOperationException($"No handler found for {signature}");

        object? result = await handler(parameters);

        return result is null ? default : (TResult)result;
    }
}

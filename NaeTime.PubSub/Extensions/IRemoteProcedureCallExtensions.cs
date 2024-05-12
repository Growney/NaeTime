using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub.Extensions;
public static class IRemoteProcedureCallExtensions
{
    public static async Task<TResult?> InvokeAsync<TResult>(this IRemoteProcedureCallClient manager, RPCSignature signature, params object?[] parameters)
    {
        Func<object?[], Task<object?>> handler = manager.GetHandler(signature) ?? throw new InvalidOperationException($"No handler found for {signature}");

        object? result = await handler(parameters);

        return result is null ? default : (TResult)result;
    }
}

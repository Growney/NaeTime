using NaeTime.PubSub.Abstractions;

public interface IRemoteProcedureCallClient
{
    Func<object?[], Task<object?>>? GetHandler(RPCSignature signature);
}
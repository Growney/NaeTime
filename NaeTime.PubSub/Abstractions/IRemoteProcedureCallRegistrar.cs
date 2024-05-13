namespace NaeTime.PubSub.Abstractions;
public interface IRemoteProcedureCallRegistrar
{
    void RegisterHandler(RPCSignature signature, Func<object?[], Task<object?>> handler);
}

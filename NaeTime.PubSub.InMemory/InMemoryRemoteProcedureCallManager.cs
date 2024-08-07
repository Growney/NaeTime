﻿using NaeTime.PubSub.Abstractions;

namespace NaeTime.PubSub.InMemory;

public class InMemoryRemoteProcedureCallManager : IRemoteProcedureCallClient, IRemoteProcedureCallRegistrar
{
    private readonly FuzzySignatureCollection _signatures = new();

    public void RegisterHandler(RPCSignature signature, Func<object?[], Task<object?>> handler) => _signatures.AddSignature(signature, handler);
    public Func<object?[], Task<object?>>? GetHandler(RPCSignature signature) => _signatures.TryFindHandler(signature, out Func<object?[], Task<object?>>? handler) ? handler : null;
}

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace NaeTime.PubSub.Abstractions;
public class FuzzySignatureCollection
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>>> _signatures = new();

    public void AddSignature(RPCSignature signature, Func<object?[], Task<object?>> handler)
    {
        if (!_signatures.TryGetValue(signature.Name, out ConcurrentDictionary<Type, ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>>? returnTypeDictionary))
        {
            returnTypeDictionary = new ConcurrentDictionary<Type, ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>>();
            _signatures.TryAdd(signature.Name, returnTypeDictionary);
        }

        if (!returnTypeDictionary.TryGetValue(signature.ReturnType, out ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>? signatureDictionary))
        {
            signatureDictionary = new ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>();
            returnTypeDictionary.TryAdd(signature.ReturnType, signatureDictionary);
        }

        signatureDictionary.AddOrUpdate(signature, handler, (key, existingHandler) => handler);
    }

    public bool TryFindHandler(RPCSignature signature, [NotNullWhen(true)] out Func<object?[], Task<object?>>? handler)
    {
        handler = null;
        if (!_signatures.TryGetValue(signature.Name, out ConcurrentDictionary<Type, ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>>? matchingNames))
        {
            return false;
        }

        if (!matchingNames.TryGetValue(signature.ReturnType, out ConcurrentDictionary<RPCSignature, Func<object?[], Task<object?>>>? matchingReturnTypes))
        {
            foreach (Type returnType in matchingNames.Keys)
            {
                if (signature.ReturnType.IsAssignableFrom(returnType))
                {
                    matchingReturnTypes = matchingNames[returnType];
                    break;
                }
            }

            if (matchingReturnTypes == null)
            {
                return false;
            }
        }

        if (!matchingReturnTypes.TryGetValue(signature, out handler))
        {
            Type[] signatureParameters = signature.ParameterTypes;
            foreach (RPCSignature fuzzySignature in matchingReturnTypes.Keys)
            {
                Type[] checkParameters = fuzzySignature.ParameterTypes;

                if (signatureParameters.Length != checkParameters.Length)
                {
                    continue;
                }

                for (int i = 0; i < signatureParameters.Length; i++)
                {
                    if (!checkParameters[i].IsAssignableFrom(signatureParameters[i]))
                    {
                        continue;
                    }
                }

                handler = matchingReturnTypes[fuzzySignature];
                break;
            }

            if (handler == null)
            {
                return false;
            }
        }

        return true;
    }

}

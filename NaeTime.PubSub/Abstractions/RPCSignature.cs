namespace NaeTime.PubSub.Abstractions;
public struct RPCSignature
{
    public RPCSignature(string name, Type returnType, Type[] parameterTypes)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        ParameterTypes = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));
    }

    public string Name { get; }
    public Type ReturnType { get; }
    public Type[] ParameterTypes { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not RPCSignature signature)
        {
            return false;
        }

        if (!EqualityComparer<Type>.Default.Equals(ReturnType, signature.ReturnType))
        {
            return false;
        }

        if (Name != signature.Name)
        {
            return false;
        }

        if (!ParameterTypes.SequenceEqual(signature.ParameterTypes))
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hash = HashCode.Combine(ReturnType, Name);

        foreach (Type parameterType in ParameterTypes)
        {
            hash = HashCode.Combine(hash, parameterType);
        }

        return hash;
    }
}

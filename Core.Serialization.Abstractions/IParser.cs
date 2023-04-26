using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Serialization.Abstractions
{
    public interface IParser
    {
        bool TryParse<T>(byte[] data, out T result);
        T Parse<T>(byte[] data);
        bool TryParse(Type t, byte[] data, out object result);
        object Parse(Type t, byte[] data);

        bool TryParse<T>(ReadOnlyMemory<byte> data, out T result);
        T Parse<T>(ReadOnlyMemory<byte> data);
        bool TryParse(Type t, ReadOnlyMemory<byte> data, out object result);
        object Parse(Type t, ReadOnlyMemory<byte> data);

        bool TryParse<T>(string data, out T result);
        T Parse<T>(string data);

        bool TryParse(Type t, string data, out object result);
        object Parse(Type t, string data);
    }
}

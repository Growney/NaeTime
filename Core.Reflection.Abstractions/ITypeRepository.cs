using System;

namespace Core.Reflection.Abstractions
{
    public interface ITypeRepository
    {
        void AddType<T>();
        Type GetType(string fullName);
        bool TryGetType(string fullname, out Type result);
    }
}

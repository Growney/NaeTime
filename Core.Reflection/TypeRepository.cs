using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Core.Reflection.Abstractions;

namespace Core.Reflection
{
    public class TypeRepository : ITypeRepository
    {
        private readonly ConcurrentDictionary<string, Type> _repo = new ConcurrentDictionary<string, Type>();
        public void AddType<T>()
        {

            var type = typeof(T);
            _repo.TryAdd(type.FullName, type);
        }

        public Type GetType(string fullName)
        {
            _repo.TryGetValue(fullName, out var retVal);
            return retVal;
        }

        public bool TryGetType(string fullname, out Type result)
        {
            return _repo.TryGetValue(fullname, out result);
        }
    }
}

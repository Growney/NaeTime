using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.Reflection
{
    public static class TypeExtensionMethods
    {
        public static bool ImplementsInterface<T>(this Type implementationType)
        {
            var interfaceType = typeof(T);
            return implementationType.ImplementsInterface(interfaceType);
        }
        public static bool ImplementsInterface(this Type implementationType, Type interfaceType)
        {
            var interfaces = implementationType.GetInterfaces();

            return DirectlyContainsInterface(interfaces, interfaceType) || GenericContains(interfaces, interfaceType);
        }

        private static bool DirectlyContainsInterface(Type[] interfaces, Type interfaceType)
        {
            return interfaces.Contains(interfaceType);
        }
        private static bool GenericContains(Type[] interfaces, Type interfaceType)
        {
            return interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);
        }

        public static Type[] GetParameterTypes(this ParameterInfo[] parameters)
        {
            Type[] types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            return types;
        }
    }
}

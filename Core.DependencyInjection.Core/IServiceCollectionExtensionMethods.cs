using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Core.DependencyInjection.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddPassthroughService<TServiceType>(this IServiceCollection collection, IServiceProvider provider)
            where TServiceType : class
        {
            collection.AddSingleton(x => provider.GetService<TServiceType>());
            return collection;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Reflection;
using Core.Reflection.Abstractions;
using EventSourcingCore.Event.Abstractions;

namespace EventSourcingCore.Event.Core
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddEventCore(this IServiceCollection collection)
        {
            collection.TryAddSingleton<IEventFactory, EventFactory>();
            collection.TryAddTransient<IMethodProviderFactory, MethodProviderFactory>();

            return collection;
        }
    }
}

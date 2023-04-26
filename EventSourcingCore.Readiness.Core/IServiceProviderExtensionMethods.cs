using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EventSourcingCore.Readiness.Abstractions;

namespace EventSourcingCore.Readiness.Core
{
    public static class IServiceProviderExtensionMethods
    {
        public static IEnumerable<Task<ReadinessResult>> GetReadinessResults(this IServiceProvider provider)
        {
            var readinessServices = provider.GetServices<IReadinessCheck>();
            if (readinessServices != null)
            {
                foreach (var service in readinessServices)
                {
                    yield return service.IsReady();
                }
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public class GlobalPrecursorRegistry : IGlobalPrecursorRegistry
    {
        private readonly List<Func<ICommandPrecursor>> _registry = new List<Func<ICommandPrecursor>>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        public GlobalPrecursorRegistry(ILogger<GlobalPrecursorRegistry> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public IEnumerable<ICommandPrecursor> GetOrderedPrecursors()
        {
            List<ICommandPrecursor> retVal = new List<ICommandPrecursor>();

            foreach (Func<ICommandPrecursor> precursorProvider in _registry)
            {
                retVal.Add(precursorProvider());
            }

            _logger?.LogTrace("Request for global precursors returned {precursorCount} results", retVal.Count);

            return retVal;
        }

        public void RegisterClass<T>() where T : ICommandPrecursor
        {
            _logger?.LogInformation("Registered {precursorType} as transient global precursor", typeof(T));

            _registry.Add(() =>
            {
                return ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            });
        }

        public void RegisterObject<T>(T instance) where T : ICommandPrecursor
        {
            _logger?.LogInformation("Registered {precursorType} as singleton global precursor", typeof(T));

            _registry.Add(() =>
            {
                return instance;
            });
        }
    }
}

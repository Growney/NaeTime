using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.Core
{
    public class CommandHandlerRegistry : ICommandHandlerRegistry
    {
        private readonly ConcurrentDictionary<string, ICommandHandler> _registry = new ConcurrentDictionary<string, ICommandHandler>();
        private readonly ILogger _logger;
        public CommandHandlerRegistry(ILogger<CommandHandlerRegistry> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> RegisteredIdentifiers => _registry.Keys;

        public ICommandHandler GetHandler(string identifier)
        {
            _registry.TryGetValue(identifier, out ICommandHandler retVal);
            return retVal;
        }

        public void RegisterHandler(ICommandHandler handler)
        {
            if (!_registry.TryAdd(handler.Identifier, handler))
            {
                _logger.LogWarning("Attempted to register handler twice for {commandIdentifier}", handler.Identifier);
                throw new InvalidOperationException("Identifier already registered");
            }
            _logger.LogInformation("Registered handler for {commandIdentifier}", handler.Identifier);
        }
    }
}

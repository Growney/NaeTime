using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Security.Abstractions;
using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;

namespace EventSourcingCore.Aggregate.Core.Factories
{
    public class SystemEventMetadataFactory : IEventMetadataFactory<SystemEventMetadata>
    {
        private readonly ICommandContextAccessor _commandContextAccessor;
        private readonly ISystemContextAccessor _systemContextAccessor;
        private readonly ILogger<SystemEventMetadataFactory> _logger;
        public SystemEventMetadataFactory(ILogger<SystemEventMetadataFactory> logger, ISystemContextAccessor systemContextAccessor, ICommandContextAccessor commandContextAccessor)
        {
            _commandContextAccessor = commandContextAccessor;
            _systemContextAccessor = systemContextAccessor;
            _logger = logger;
        }
        public SystemEventMetadata CreateMetadata(IEvent eventObj)
        {
            if (_commandContextAccessor?.Context?.Metadata == null)
            {
                throw new InvalidOperationException("No command metadata found found");
            }
            var commandMetadata = _commandContextAccessor.Context.Metadata;

            if (_systemContextAccessor?.Context.TokenID == null || _systemContextAccessor.Context.TokenID == Guid.Empty)
            {
                throw new InvalidOperationException("No system context found");
            }

            string identifier = eventObj.GetIdentifier();

            _logger.LogTrace("Creating system event metadata for {Identifier}-{TokenID}", identifier, _systemContextAccessor.Context.TokenID);
            return new SystemEventMetadata(Guid.NewGuid(), identifier, commandMetadata.CreatedAt, commandMetadata.ValidFrom, _systemContextAccessor.Context.TokenID);
        }
    }
}

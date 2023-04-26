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
    public class CustomerEventMetadataFactory : IEventMetadataFactory<CustomerEventMetadata>
    {
        private readonly ICustomerContextAccessor _customerContextAccessor;
        private readonly ICommandContextAccessor _commandContextAccessor;
        private readonly IUserContextAccessor _userContextAccessor;
        private readonly ILogger<CustomerEventMetadataFactory> _logger;
        public CustomerEventMetadataFactory(ILogger<CustomerEventMetadataFactory> logger, IUserContextAccessor userContextAccessor, ICommandContextAccessor commandContextAccessor, ICustomerContextAccessor customerContextAccessor)
        {
            _commandContextAccessor = commandContextAccessor;
            _customerContextAccessor = customerContextAccessor;
            _userContextAccessor = userContextAccessor;
            _logger = logger;
        }
        public CustomerEventMetadata CreateMetadata(IEvent eventObj)
        {
            if (_commandContextAccessor?.Context?.Metadata == null)
            {
                throw new InvalidOperationException("No command metadata found found");
            }
            var commandMetadata = _commandContextAccessor.Context.Metadata;
            if (_customerContextAccessor?.Context.CustomerID == null || _customerContextAccessor.Context.CustomerID == Guid.Empty)
            {
                throw new InvalidOperationException("No customer context found");
            }
            Guid userID = Guid.Empty;
            if (_userContextAccessor?.Context.UserID != null)
            {
                userID = _userContextAccessor.Context.UserID;
            }

            var userContext = _userContextAccessor.Context;
            var customerContext = _customerContextAccessor.Context;
            string identifier = eventObj.GetIdentifier();

            _logger.LogTrace("Creating customer event metadata for {Identifier}-{UserId}-{CustomerID}", identifier, userID, customerContext.CustomerID);
            return new CustomerEventMetadata(Guid.NewGuid(), identifier, commandMetadata.CreatedAt, commandMetadata.ValidFrom, userID, customerContext.CustomerID);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Core.Serialization.Json;
using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.CommandHandler.Core;
using System.Net;
using EventSourcingCore.Store.Abstractions;
using EventSourcingCore.Aggregate.Abstractions;
using Microsoft.Extensions.Logging;
using EventSourcingCore.CommandHandler.ASPNET;

namespace EventSourcingCore.CommandHandler.ASPNET
{
    public class DefaultHttpRequestCommandHandler : IHttpRequestCommandHandler
    {
        private readonly IOptions<WebCommandHandlerOptions> _options;
        private readonly ICommandHandlerRegistry _registry;
        private readonly ILogger<DefaultHttpRequestCommandHandler> _logger;
        public DefaultHttpRequestCommandHandler(ICommandHandlerRegistry registry, IOptions<WebCommandHandlerOptions> options)
        : this(registry, options, null)
        {

        }
        public DefaultHttpRequestCommandHandler(ICommandHandlerRegistry registry, IOptions<WebCommandHandlerOptions> options, ILogger<DefaultHttpRequestCommandHandler> logger)
        {
            _registry = registry;
            _options = options;
            _logger = logger;
        }
        public async Task Handle(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            try
            {
                _logger?.LogInformation("{httpMethod} handle request received from {sourceIp}:{sourcePort}", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort);
                if (context.Request.Method != "POST")
                {
                    _logger?.LogWarning("{httpMethod} handle request received from {sourceIp}:{sourcePort} has incorrect HTTP method", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
                if (string.IsNullOrWhiteSpace(_options.Value.MetadataHeader))
                {
                    _logger?.LogCritical("Handler metadata header name undefined");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync("Metadata_Header_Undefined");
                    return;
                }
                HttpRequest request = context.Request;


                Microsoft.Extensions.Primitives.StringValues metadataString = context.Request.Headers[_options.Value.MetadataHeader];

                if (!TryGetCommandMetadata(metadataString, out CommandMetadata metadata))
                {
                    _logger?.LogWarning("{httpMethod} handle request received from {sourceIp}:{sourcePort} has badly formatted command metadata", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("Invalid_Metadata");
                    return;
                }
                ICommandHandler handler = _registry.GetHandler(metadata.CommandIdentifier);

                if (handler == null)
                {
                    _logger?.LogWarning("{httpMethod} handle request received from {sourceIp}:{sourcePort} has an unsupported command identifier {commandIdentifier}", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort, metadata.CommandIdentifier);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync("Command_Identifier_Not_Found");
                    return;
                }

                ICommand command = await context.Request.GetCommand(handler.CommandType);
                CommandContext commandContext = new CommandContext(metadata, command, context.RequestServices);

                try
                {
                    _logger?.LogInformation("Executing command {commmandIdentifier} from {sourceIp}:{sourcePort}", metadata.CommandIdentifier, context.Connection.RemoteIpAddress, context.Connection.RemotePort);
                    await handler.Execute(commandContext);

                    context.Response.StatusCode = (int)HttpStatusCode.Accepted;

                    _logger?.LogInformation("{httpMethod} handle request received from {sourceIp}:{sourcePort} with identifier {commandIdentifier} has been accepted", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort, metadata.CommandIdentifier);
                }
                catch (DomainException ex)
                {
                    _logger?.LogInformation("{httpMethod} handle request received from {sourceIp}:{sourcePort} has an unsupported command identifier {commandIdentifier} has failed with a domain violation: {message}", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort, metadata.CommandIdentifier, ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    await context.Response.WriteAsync(ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger?.LogInformation("{httpMethod} handle request received from {sourceIp}:{sourcePort} has an unsupported command identifier {commandIdentifier} has failed authorization: {message}", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort, metadata.CommandIdentifier, ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync(ex.Message);
                }
                catch (ConcurrencyException ex)
                {
                    _logger?.LogInformation("{httpMethod} handle request received from {sourceIp}:{sourcePort} has an unsupported command identifier {commandIdentifier} has failed due to a concurrency issue: expected version: {expectedVersion} actual version: {actualVersion}", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort, metadata.CommandIdentifier, ex.ExpectedVersion, ex.ActualVersion);
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    await context.Response.WriteAsync(ex.Message);
                }

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "{httpMethod} handle request received from {sourceIp}:{sourcePort} threw an exception", context.Request.Method, context.Connection.RemoteIpAddress, context.Connection.RemotePort);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(ex.Message);
            }
        }

        private bool TryGetCommandMetadata(string headerValue, out CommandMetadata result)
        {
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                JsonParser parser = new JsonParser();
                byte[] headerBytes = Convert.FromBase64String(headerValue);
                return parser.TryParse(headerBytes, out result);
            }
            else
            {
                result = new CommandMetadata();
                return false;
            }
        }

    }
}

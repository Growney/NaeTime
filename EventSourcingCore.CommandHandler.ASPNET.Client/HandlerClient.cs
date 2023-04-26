using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Serialization.Json;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.ASPNET.Client
{
    public class HandlerClient : IHandlerClient
    {
        private readonly Dictionary<string, HandlerServiceOptions> _mainClients = new Dictionary<string, HandlerServiceOptions>();

        private readonly HandlerClientOptions _options;
        private readonly IHttpClientFactory _clientFactory;
        protected readonly ZonedClock _clock;

        public HandlerClient(IHttpClientFactory clientFactory, IOptions<HandlerClientOptions> configuration)
        {
            _options = configuration.Value;
            AddClients(configuration?.Value);
            _clientFactory = clientFactory;


            _clock = new ZonedClock(SystemClock.Instance, DateTimeZoneProviders.Tzdb.GetSystemDefault(), CalendarSystem.Iso);
        }

        private void AddClients(HandlerClientOptions options)
        {
            if (options != null && options.Services != null)
            {
                foreach (var service in options.Services)
                {
                    foreach (var identifier in service.Identifiers)
                    {
                        if (_mainClients.ContainsKey(identifier))
                        {
                            throw new ArgumentException($"Multiple main clients provided for {identifier}");
                        }
                        else
                        {
                            _mainClients.Add(identifier, service);
                        }
                    }
                }
            }
        }


        private CommandMetadata CreateMetadata(string identifier, ZonedDateTime? validFrom)
        {
            var current = _clock.GetCurrentZonedDateTime();
            return new CommandMetadata(identifier, current, validFrom ?? current);
        }
        private string CreateMetadataHeader(string identifier, ZonedDateTime? validFrom)
        {
            var serializer = new JsonSerializer();
            var metadata = CreateMetadata(identifier, validFrom);
            var metadataJson = serializer.SerializeToString(metadata);
            var encodedJson = Encoding.UTF8.GetBytes(metadataJson);
            var base64Metadata = Convert.ToBase64String(encodedJson);

            return base64Metadata;
        }

        private string CreateCommandJson(object command)
        {
            var serializer = new JsonSerializer();
            return serializer.SerializeToString(command);
        }

        private HttpRequestMessage CreateMessage(string identifier, object command, ZonedDateTime? validFrom, string authenticationToken, string path, string metadataHeader)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, path);
            message.Headers.Add(metadataHeader, CreateMetadataHeader(identifier, validFrom));
            if (!string.IsNullOrWhiteSpace(authenticationToken))
            {
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationToken);
            }
            message.Content = new StringContent(CreateCommandJson(command), Encoding.UTF8, "application/json");
            return message;
        }

        private async Task<HttpResponseMessage> SendCommandAsync(string identifier, object command, ZonedDateTime? validFrom, string authenticationToken, string address, string path, string metadataHeader)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(address);
                var message = CreateMessage(identifier, command, validFrom, authenticationToken, path, metadataHeader);

                var response = await httpClient.SendAsync(message);

                return response;
            }
        }

        public Task<HttpResponseMessage> SendCommandAsync(string identifier, object command, ZonedDateTime? validFrom = null, string authenticationToken = null, string address = null)
        {
            HandlerServiceOptions handler = null;
            if (address == null && !_mainClients.TryGetValue(identifier, out handler))
            {
                throw new ArgumentException($"Handler for identifier {identifier} not found");
            }

            string mainAddress = address ?? handler.Address;
            string mainPath = handler?.Path ?? _options.Path ?? Constants.HANDLE_PATH;
            string mainHeader = handler?.MetadataHeader ?? _options.MetadataHeader ?? Constants.METADATA_HEADER;

            return SendCommandAsync(identifier, command, validFrom, authenticationToken, mainAddress, mainPath, mainHeader);
        }



    }
}

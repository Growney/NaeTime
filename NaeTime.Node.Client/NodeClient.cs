using NaeTime.Node.Client.Abstractions;
using NaeTime.Shared;
using NaeTime.Shared.Node;
using System.Net.Http;
using System.Net.Http.Json;

namespace NaeTime.Node.Client
{
    public class NodeClient : INodeClient
    {
        private readonly HttpClient _client;

        public NodeClient(HttpClient client)
        {
            _client = client;
        }

        public async Task ConfigureAsync(ConfigurationDto dto)
        {
            var response = await _client.PostAsJsonAsync("/config", dto);

            response.EnsureSuccessStatusCode();
        }

        public void Dispose()=>_client.Dispose();

        public Task<ConfigurationDto?> GetConfigurationAsync() => _client.GetFromJsonAsync<ConfigurationDto>("/config");

        public async Task<List<RssiReceiverDto>> GetReceiversAsync() => await _client.GetFromJsonAsync<List<RssiReceiverDto>>("/receivers") ?? new List<RssiReceiverDto>();
        public Task<RssiStreamDto?> GetRssiStreamAsync(Guid streamId) => _client.GetFromJsonAsync<RssiStreamDto>($"rssistream/{streamId}");
        public async Task<RssiStreamDto?> StartRssiStreamAsync(Guid streamId, int frequency, RssiReceiverTypeDto? receiverType)
        {
            var queryString = $"rssistream/{streamId}/start?frequency={frequency}";
            if(receiverType != null)
            {
                queryString += $"&receiverType={receiverType}";
            }

            var responseMessage = await _client.PostAsync(queryString,null);
            
            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadFromJsonAsync<RssiStreamDto>();
        }

        public async Task StopRssiStreamAsync(Guid streamId)
        {
            var response = await _client.PostAsync($"/rssistream/{streamId}/stop",null);

            response.EnsureSuccessStatusCode();
        }
    }
}
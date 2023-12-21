using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Shared.DataTransferObjects;
using System.Net.Http.Json;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class WebApiHardwareClient : IHardwareApiClient
{
    private readonly HttpClientProvider _httpClientProvider;

    public WebApiHardwareClient(HttpClientProvider httpClientProvider)
    {
        _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
    }
    private EthernetLapRF8Channel GetDomainFromDto(EthernetLapRF8ChannelTimerDetails details)
        => new EthernetLapRF8Channel()
        {
            Id = details.Id,
            IpAddress = details.IpAddress,
            Name = details.Name,
            Port = details.Port,
        };
    private Models.TimerDetails GetDomainFromDto(Shared.DataTransferObjects.TimerDetails details)
        => new Models.TimerDetails()
        {
            Id = details.Id,
            Name = details.Name,
            Type = GetDomainFromDto(details.Type)
        };
    private Models.TimerType GetDomainFromDto(Shared.DataTransferObjects.TimerType type)
        => type switch
        {
            Shared.DataTransferObjects.TimerType.EthernetLapRF8Channel => Models.TimerType.EthernetLapRF8Channel,
            _ => throw new NotImplementedException()
        };
    public async Task<IEnumerable<Models.TimerDetails>> GetAllTimerDetailsAsync()
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync("hardware/timer/details/all");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return Enumerable.Empty<Models.TimerDetails>();
        }

        var responseDtos = await response.Content.ReadFromJsonAsync<IEnumerable<Shared.DataTransferObjects.TimerDetails>>();

        if (responseDtos == null)
        {
            return Enumerable.Empty<Models.TimerDetails>();
        }

        return responseDtos.Select(GetDomainFromDto);
    }

    public async Task<EthernetLapRF8Channel?> CreateEthernetLapRF8ChannelAsync(string name, string? ipAddress, int port)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException("Ip_address_is_required");
        }

        if (port < 0 || port > 65535)
        {
            throw new InvalidOperationException("Port_is_invalid");
        }

        var dto = new CreateEthernetLapRF8ChannelTimer(name, ipAddress, (ushort)port);

        var content = JsonContent.Create(dto);
        var response = await client.PostAsync("hardware/ethernetlaprf8channel/create", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<EthernetLapRF8ChannelTimerDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }
    public async Task UpdateEthernetLapRF8ChannelAsync(EthernetLapRF8Channel timer)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        if (string.IsNullOrWhiteSpace(timer.IpAddress))
        {
            throw new InvalidOperationException("Ip_address_is_required");
        }

        if (timer.Port < 0 || timer.Port > 65535)
        {
            throw new InvalidOperationException("Port_is_invalid");
        }

        var dto = new EthernetLapRF8ChannelTimerDetails(timer.Id, timer.Name, timer.IpAddress, (ushort)timer.Port);

        var content = JsonContent.Create(dto);
        var response = await client.PutAsync("hardware/ethernetlaprf8channel/update", content);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException("Failed_to_update");
        }
    }
    public async Task<EthernetLapRF8Channel?> GetEthernetLapRF8ChannelDetailsAsync(Guid id)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync($"hardware/ethernetlaprf8channel/{id}");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<EthernetLapRF8ChannelTimerDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }
}

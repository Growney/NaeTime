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
    private LapRF8Channel GetDomainFromDto(LapRF8ChannelTimerDetails details)
        => new LapRF8Channel()
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
            Shared.DataTransferObjects.TimerType.LapRF8Channel => Models.TimerType.LapRF8Channel,
            _ => throw new NotImplementedException()
        };
    public async Task<LapRF8Channel?> CreateLapRF8ChannelAsync(string name, int ipAddress, ushort port)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var dto = new CreateLapRF8ChannelTimer(name, ipAddress, port);

        var content = JsonContent.Create(dto);
        var response = await client.PostAsync("hardware/laprf8channel/create", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<LapRF8ChannelTimerDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<IEnumerable<LapRF8Channel>> GetAllLapRF8ChannelAsync()
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync("hardware/laprf8channel/all");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return Enumerable.Empty<LapRF8Channel>();
        }

        var responseDtos = await response.Content.ReadFromJsonAsync<IEnumerable<LapRF8ChannelTimerDetails>>();

        if (responseDtos == null)
        {
            return Enumerable.Empty<LapRF8Channel>();
        }

        return responseDtos.Select(GetDomainFromDto);
    }
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

    public async Task<LapRF8Channel?> GetLapRF8ChannelDetailsAsync(Guid id)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync($"hardware/laprf8channel/{id}");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<LapRF8ChannelTimerDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }
}

using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Shared.DataTransferObjects.Pilot;
using System.Net.Http.Json;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class WebApiPilotClient : IPilotApiClient
{
    private HttpClientProvider _httpClientProvider;

    public WebApiPilotClient(HttpClientProvider httpClientProvider)
    {
        _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
    }
    private Pilot GetDomainFromDto(PilotDetails dto)
        => new Pilot(dto.Id, dto.FirstName, dto.LastName, dto.CallSign);
    public async Task<Pilot?> CreateAsync(string? firstname, string? lastname, string? callsign)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var dto = new CreatePilot(firstname, lastname, callsign);

        var content = JsonContent.Create(dto);
        var response = await client.PostAsync("pilot/create", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<PilotDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<IEnumerable<Pilot>> GetAllAsync()
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }
        var response = await client.GetAsync("pilot/all");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return Enumerable.Empty<Pilot>();
        }

        var responseDtos = await response.Content.ReadFromJsonAsync<IEnumerable<PilotDetails>>();

        if (responseDtos == null)
        {
            return Enumerable.Empty<Pilot>();
        }

        return responseDtos.Select(GetDomainFromDto);
    }

    public async Task<Pilot?> GetAsync(Guid id)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync($"pilot/{id}");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<PilotDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<Pilot?> UpdateAsync(Pilot update)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var dto = new UpdatePilot(update.Id, update.FirstName, update.LastName, update.CallSign);

        var content = JsonContent.Create(dto);
        var response = await client.PutAsync("pilot/update", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<PilotDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }
}

using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Shared.DataTransferObjects;
using System.Net.Http.Json;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class WebApiPilotClient : IPilotApiClient
{
    private IHttpClientProvider _httpClientProvider;

    public WebApiPilotClient(IHttpClientProvider httpClientProvider)
    {
        _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
    }
    private Pilot GetDomainFromDto(PilotDetails dto)
        => new Pilot()
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CallSign = dto.CallSign,
        };
    public async Task<Pilot?> CreatePilotAsync(string? firstname, string? lastname, string? callsign)
    {
        var client = _httpClientProvider.GetHttpClient();
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

    public async Task<IEnumerable<Pilot>> GetAllPilotsAsync()
    {
        var client = _httpClientProvider.GetHttpClient();

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

    public async Task<Pilot?> GetPilotDetailsAsync(Guid id)
    {
        var client = _httpClientProvider.GetHttpClient();

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
}

using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Shared.DataTransferObjects.Track;
using System.Net.Http.Json;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class WebApiTrackClient : ITrackApiClient
{
    private HttpClientProvider _httpClientProvider;

    public WebApiTrackClient(HttpClientProvider httpClientProvider)
    {
        _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
    }

    private Track GetDomainFromDto(TrackDetails details)
    {
        var sortedGates = details.TimedGates.OrderBy(x => x.OrdinalPosition);
        return new Track(details.Id, details.Name, sortedGates.Select(x => x.TimerId));
    }

    public async Task<Track?> CreateAsync(string name, IEnumerable<TimedGate> gates)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        int ordinalPosition = -1;
        var gatesDto = gates.Select(g => new TimedGateDetails(ordinalPosition++, g.TimerId));

        var dto = new CreateTrack(name, gatesDto);

        var content = JsonContent.Create(dto);
        var response = await client.PostAsync("track/create", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<TrackDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }
        var response = await client.GetAsync("track/all");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return Enumerable.Empty<Track>();
        }

        var responseDtos = await response.Content.ReadFromJsonAsync<IEnumerable<TrackDetails>>();

        if (responseDtos == null)
        {
            return Enumerable.Empty<Track>();
        }

        return responseDtos.Select(GetDomainFromDto);
    }

    public async Task<Track?> GetAsync(Guid id)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync($"track/{id}");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<TrackDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<Track?> UpdateAsync(Track update)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }
        int ordinalPosition = -1;
        var gatesDto = update.TimedGates.Select(g => new TimedGateDetails(ordinalPosition++, g.TimerId));
        var dto = new UpdateTrack(update.Id, update.Name, gatesDto);

        var content = JsonContent.Create(dto);
        var response = await client.PutAsync("track/update", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<TrackDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }
}

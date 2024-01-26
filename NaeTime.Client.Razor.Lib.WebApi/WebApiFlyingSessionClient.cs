using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Shared.DataTransferObjects.FlyingSession;
using System.Net.Http.Json;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class WebApiFlyingSessionClient : IFlyingSessionApiClient
{
    private HttpClientProvider _httpClientProvider;

    public WebApiFlyingSessionClient(HttpClientProvider httpClientProvider)
    {
        _httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));
    }

    private FlyingSession GetDomainFromDto(FlyingSessionDetails dto)
        => new FlyingSession(dto.Id, dto.Description, dto.Start, dto.ExpectedEnd, dto.TrackId);

    public async Task<FlyingSession?> CreateAsync(string description, DateTime start, DateTime expectedEnd, Guid trackId)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var dto = new CreateFlyingSession(description, start, expectedEnd, trackId);

        var content = JsonContent.Create(dto);
        var response = await client.PostAsync("flyingsession/create", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<FlyingSessionDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<IEnumerable<FlyingSession>> GetAllAsync()
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }
        var response = await client.GetAsync("flyingsession/all");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return Enumerable.Empty<FlyingSession>();
        }

        var responseDtos = await response.Content.ReadFromJsonAsync<IEnumerable<FlyingSessionDetails>>();

        if (responseDtos == null)
        {
            return Enumerable.Empty<FlyingSession>();
        }

        return responseDtos.Select(GetDomainFromDto);
    }

    public async Task<FlyingSession?> GetAsync(Guid id)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        var response = await client.GetAsync($"flyingsession/{id}");

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<FlyingSessionDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }

    public async Task<FlyingSession?> UpdateAsync(FlyingSession flyingSession)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            throw new InvalidOperationException("Client_not_configured");
        }

        if (flyingSession.TrackId == Guid.Empty)
        {
            throw new InvalidOperationException("Track_not_configured");
        }

        var dto = new UpdateFlyingSession(flyingSession.Id, flyingSession.Description ?? string.Empty, flyingSession.Start, flyingSession.ExpectedEnd, flyingSession.Id);

        var content = JsonContent.Create(dto);
        var response = await client.PutAsync("flyingsession/update", content);

        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            return null;
        }

        var responseDto = await response.Content.ReadFromJsonAsync<FlyingSessionDetails>();

        if (responseDto == null)
        {
            return null;
        }

        return GetDomainFromDto(responseDto);
    }
}

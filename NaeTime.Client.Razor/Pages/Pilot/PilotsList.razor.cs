using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Pages.Pilot;
public partial class PilotsList : ComponentBase
{
    [Inject]
    private ILocalApiClientProvider ClientProvider { get; set; } = null!;

    private readonly List<Lib.Models.Pilot> _pilots = new();

    protected override async Task OnInitializedAsync()
    {
        var existingPilots = await ClientProvider.PilotApiClient.GetAllPilotsAsync();

        _pilots.AddRange(existingPilots);

        await base.OnInitializedAsync();
    }
}

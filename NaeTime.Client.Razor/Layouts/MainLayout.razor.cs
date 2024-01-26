using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Layouts;
public partial class MainLayout
{
    [Inject]
    public ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    public IOffSiteApiClientProvider OffSiteApiClientProvider { get; set; } = null!;

    [Inject]
    public ILocalApiConfiguration LocalApiConfiguration { get; set; } = null!;
    [Inject]
    public IOffSiteApiConfiguration OffSiteApiConfiguration { get; set; } = null!;

    private bool IsLocalEnabled { get; set; } = false;
    private bool IsOffSiteEnabled { get; set; } = false;
    protected override async Task OnInitializedAsync()
    {
        IsLocalEnabled = await LocalApiConfiguration.IsEnabledAsync();
        IsOffSiteEnabled = await OffSiteApiConfiguration.IsEnabledAsync();

        await base.OnInitializedAsync();
    }
}
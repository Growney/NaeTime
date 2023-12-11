using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Layouts;
public partial class NavMenu : ComponentBase
{
    [Inject]
    public ILocalApiClientProvider LocalClientProvider { get; set; } = null!;
    [Inject]
    public IOffSiteApiClientProvider OffSiteClientProvider { get; set; } = null!;

    public bool IsLocalEnabled { get; set; } = false;
    public bool IsOffSiteEnabled { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        IsLocalEnabled = await LocalClientProvider.IsEnabledAsync(CancellationToken.None);
        IsOffSiteEnabled = await OffSiteClientProvider.IsEnabledAsync(CancellationToken.None);
        await base.OnParametersSetAsync();
    }
}

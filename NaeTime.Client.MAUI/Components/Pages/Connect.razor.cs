using Microsoft.AspNetCore.Components;
using NaeTime.Client.MAUI.Lib;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.MAUI.Components.Pages;
public partial class Connect : ComponentBase
{

    [Inject]
    public ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    public IOffSiteApiClientProvider OffSiteApiClientProvider { get; set; } = null!;

    [Inject]
    public ILocalApiConfiguration LocalConfiguration { get; set; } = null!;
    [Inject]
    public IOffSiteApiConfiguration OffsiteConfiguration { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    private ConnectionState LocalConnectionState { get; set; } = ConnectionState.Connecting;
    private ConnectionState OffSiteConnectionState { get; set; } = ConnectionState.Connecting;

    private bool IsSuccessState => (LocalConnectionState == ConnectionState.Disabled || LocalConnectionState == ConnectionState.Success)
            &&
            (OffSiteConnectionState == ConnectionState.Disabled || OffSiteConnectionState == ConnectionState.Success);
    private void MoveToNextPage()
    {
        NavigationManager.NavigateTo("/overview");
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _ = TestConnections();
    }
    private async Task TestConnections()
    {
        LocalConnectionState = ConnectionState.Connecting;
        OffSiteConnectionState = ConnectionState.Connecting;

        var localTestTask = TestLocalConnection();
        var offSiteTestTask = TestOffSiteConnection();

        await Task.WhenAll(localTestTask, offSiteTestTask);

        if (IsSuccessState)
        {
            await Task.Delay(1500);
            MoveToNextPage();
        }
    }
    private async Task TestLocalConnection()
    {
        var state = await TestConnection(LocalApiClientProvider);
        LocalConnectionState = state;
        StateHasChanged();
    }
    private async Task TestOffSiteConnection()
    {
        var state = await TestConnection(OffSiteApiClientProvider);
        OffSiteConnectionState = state;
        StateHasChanged();
    }
    private void Retry()
    {
        _ = TestConnections();
    }
    private async Task DisableAndContinue()
    {
        if (LocalConnectionState == ConnectionState.ConnectionFailed
            && OffSiteConnectionState == ConnectionState.Success)
        {
            await LocalConfiguration.SetEnabledAsync(false);
            MoveToNextPage();
            return;
        }
        else if (LocalConnectionState == ConnectionState.Success
            && OffSiteConnectionState == ConnectionState.ConnectionFailed)
        {
            await OffsiteConfiguration.SetEnabledAsync(false);
            MoveToNextPage();
            return;
        }
    }
    private void BackToConfiguration()
    {
        NavigationManager.NavigateTo("/?force=true");
    }
    private async Task<ConnectionState> TestConnection(IApiClientProvider clientProvider)
    {
        var isEnabled = await clientProvider.IsEnabledAsync(CancellationToken.None);

        if (!isEnabled)
        {
            return ConnectionState.Disabled;
        }
        await Task.Delay(500);

        var isValid = await clientProvider.IsValidAsync(CancellationToken.None);

        if (!isValid)
        {
            return ConnectionState.Invalid;
        }

        await Task.Delay(500);

        var isAbleToConnect = await clientProvider.TryConnectionAsync(CancellationToken.None);

        if (!isAbleToConnect)
        {
            return ConnectionState.ConnectionFailed;
        }

        return ConnectionState.Success;
    }
}

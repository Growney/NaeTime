using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.MAUI.Components.Pages;
public partial class Connect : ComponentBase
{
    private enum ConnectionState
    {
        Connecting,
        Disabled,
        Invalid,
        ConnectionFailed,
        Success
    }
    [Inject]
    public ILocalApiClientProvider LocalApiClientProvider { get; set; } = null!;
    [Inject]
    public IOffSiteApiClientProvider OffSiteApiClientProvider { get; set; } = null!;
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
    }
    private async Task TestOffSiteConnection()
    {
        var state = await TestConnection(OffSiteApiClientProvider);
        OffSiteConnectionState = state;
    }
    private async Task<ConnectionState> TestConnection(IApiClientProvider clientProvider)
    {
        var isEnabled = await clientProvider.IsValidAsync(CancellationToken.None);

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

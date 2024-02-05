using Microsoft.AspNetCore.Components;
using NaeTime.Persistence.Client.Messages.Events;

namespace NaeTime.Client.MAUI.Components.Pages;
public partial class Configure : ComponentBase
{
    [Inject]
    public PubSub.Abstractions.IDispatcher Dispatcher { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await Dispatcher.Dispatch(new ClientModeConfigured(Persistence.Client.ClientMode.Local));
        await base.OnInitializedAsync();

        NavigationManager.NavigateTo("/overview");
    }
}

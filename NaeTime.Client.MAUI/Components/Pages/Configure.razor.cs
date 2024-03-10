using Microsoft.AspNetCore.Components;

namespace NaeTime.Client.MAUI.Components.Pages;
public partial class Configure : ComponentBase
{
    [Inject]
    public PubSub.Abstractions.IDispatcher Dispatcher { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        NavigationManager.NavigateTo("/overview");
    }
}

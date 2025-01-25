using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class EditableSerialEsp32NodeTimerDetails
{
    [Inject]
    public IEventRegistrarScope EventRegistrar { get; set; } = null!;
    [Inject]
    public IRemoteProcedureCallClient RpcClient { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Func<SerialEsp32Node, Task> OnValidSubmit { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public SerialEsp32Node Details { get; set; } = null!;
    private EditContext? _editContext;


    protected override async Task OnInitializedAsync()
    {
        EventRegistrar.RegisterHub(this);

        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        _editContext = new EditContext(Details);
        base.OnParametersSet();
    }
    private Task HandleValidSubmit() => OnValidSubmit?.Invoke(Details) ?? Task.CompletedTask;
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Components.HardwareComponents;
public partial class EditableSerialEsp32NodeTimerDetails : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public Func<SerialEsp32Node, Task> OnValidSubmit { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public SerialEsp32Node Details { get; set; } = null!;
    private EditContext? _editContext;

    protected override void OnParametersSet()
    {
        _editContext = new EditContext(Details);
        base.OnParametersSet();
    }
    private Task HandleValidSubmit() => OnValidSubmit?.Invoke(Details) ?? Task.CompletedTask;
}

using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Components;
public partial class PilotDetails : ComponentBase
{
    [Parameter]
    public Pilot? Details { get; set; }
}

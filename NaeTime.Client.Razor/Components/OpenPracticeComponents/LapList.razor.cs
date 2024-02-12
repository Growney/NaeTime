using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class LapList : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public IEnumerable<OpenPracticeLap> Laps { get; set; } = Enumerable.Empty<OpenPracticeLap>();

    [Parameter]
    public bool IncludePilot { get; set; }
}

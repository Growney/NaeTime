using Microsoft.AspNetCore.Components;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class OpenPracticeSingleLapLeaderboard : ComponentBase
{
    [Parameter]
    public Guid SessionId { get; set; }

}

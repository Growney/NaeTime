using Microsoft.AspNetCore.Components;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class ConsecutiveLapsLeaderboard
{
    [Parameter]
    [EditorRequired]
    public Lib.Models.OpenPractice.ConsecutiveLapsLeaderboard Leaderboard { get; set; } = null!;
}

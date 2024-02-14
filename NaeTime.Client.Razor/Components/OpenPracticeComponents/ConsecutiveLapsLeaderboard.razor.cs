using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class ConsecutiveLapsLeaderboard
{
    [Parameter]
    [EditorRequired]
    public Lib.Models.OpenPractice.ConsecutiveLapsLeaderboard Leaderboard { get; set; } = null!;
    [Parameter]
    public IEnumerable<OpenPracticeLap> AllLaps { get; set; } = Enumerable.Empty<OpenPracticeLap>();
}

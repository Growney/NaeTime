using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class ConsecutiveLapsLeaderboard
{
    [Parameter]
    public IEnumerable<ConsecutiveLapsLeaderboardPosition> Positions { get; set; } = Enumerable.Empty<ConsecutiveLapsLeaderboardPosition>();
}

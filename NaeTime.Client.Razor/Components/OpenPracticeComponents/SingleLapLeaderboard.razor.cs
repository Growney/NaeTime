using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SingleLapLeaderboard
{
    [Parameter]
    public IEnumerable<SingleLapLeaderboardPosition> Positions { get; set; } = Enumerable.Empty<SingleLapLeaderboardPosition>();

}

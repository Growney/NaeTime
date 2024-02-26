﻿using Microsoft.AspNetCore.Components;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class ConsecutiveLapsLeaderboard
{
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }
    [Parameter]
    public uint LapCap { get; set; }
}

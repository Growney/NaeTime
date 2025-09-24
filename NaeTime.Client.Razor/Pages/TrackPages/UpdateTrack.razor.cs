using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Command.Abstractions;
using NaeTime.Query.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class UpdateTrack
{
    [Inject]
    private ITrackQueryHandler TrackQueryHandler { get; set; } = null!;
    [Inject]
    private IHardwareQueryHandler HardwareQueryHandler { get; set; } = null!;
    [Inject]
    private ITrackCommandHandler TrackCommandHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid TrackId { get; set; }
    [Parameter]
    public string? ReturnUrl { get; set; }

    private Track? _model;

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        var track = await TrackQueryHandler.GetTrack(TrackId);

        if (track == null)
        {
            NavigationManager.NavigateTo("/track/list");
            return;
        }

        _model = new Track()
        {
            Id = track.Id,
            Name = track.Name,
            MaximumLapTimeMilliseconds = track.MaximumLapTimeMilliseconds,
            MinimumLapTimeMilliseconds = track.MinimumLapTimeMilliseconds,
        };
        _model.AddTimers(track.Detectors.Select(x => x.Id));

        IEnumerable<Query.Abstractions.Models.Detector> timers = await HardwareQueryHandler.GetAllDetectors();

        if (timers == null)
        {
            return;
        }

        _timers.AddRange(timers.Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                Query.Abstractions.Models.DetectorType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                Query.Abstractions.Models.DetectorType.NaeTimeSerial => TimerType.SerialEsp32Node,
                _ => throw new NotImplementedException()
            }, x.SupportedLanes)));

        await base.OnInitializedAsync();
    }

    private async Task HandleValidSubmit(Track track)
    {
        await TrackCommandHandler.RenameTrack(track.Id, track.Name);
        await TrackCommandHandler.ReorderTrackDetectors(track.Id, track.Timers.ToArray());
        if (track.MaximumLapTimeMilliseconds.HasValue)
        {
            await TrackCommandHandler.SetMaximumLapTime(track.Id, track.MaximumLapTimeMilliseconds.Value);
        }
        else
        {
            await TrackCommandHandler.ResetMaximumLapTime(track.Id);
        }
        if (track.MinimumLapTimeMilliseconds.HasValue)
        {
            await TrackCommandHandler.SetMinimumLapTime(track.Id, track.MinimumLapTimeMilliseconds.Value);
        }
        else
        {
            await TrackCommandHandler.ResetMinimumLapTime(track.Id);
        }

        string returnUrl = ReturnUrl ?? "/track/list";

        NavigationManager.NavigateTo(returnUrl);
    }
}

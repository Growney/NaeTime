using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Command.Abstractions;
using NaeTime.Query.Abstractions;

namespace NaeTime.Client.Razor.Pages.TrackPages;
public partial class CreateTrack
{
    [Inject]
    private ITrackCommandHandler TrackCommandHandler { get; set; } = null!;
    [Inject]
    private IHardwareQueryHandler HardwareQueryHandler { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string? ReturnUrl { get; set; }

    private readonly Track _model = new()
    {
        Id = Guid.NewGuid(),
        Name = null
    };

    private readonly List<TimerDetails> _timers = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        IEnumerable<Query.Abstractions.Models.Detector> timersResponse = await HardwareQueryHandler.GetAllDetectors();

        if (timersResponse == null)
        {
            return;
        }

        _timers.AddRange(timersResponse.Where(x => x.Type != Hardware.Messages.Models.TimerType.SerialELRSBackpack).Select(x => new TimerDetails(x.Id, x.Name,
            x.Type switch
            {
                Query.Abstractions.Models.DetectorType.EthernetLapRF8Channel => TimerType.EthernetLapRF8Channel,
                Query.Abstractions.Models.DetectorType.NaeTimeSerial => TimerType.SerialEsp32Node,
                _ => throw new NotImplementedException()
            }, x.SupportedLanes)));

    }

    private async Task HandleValidSubmit(Track track)
    {
        await TrackCommandHandler.DesignTrack(track.Id, track.Name, track.Timers.ToArray());

        NavigationManager.NavigateTo(ReturnUrl ?? "/track/list");
    }
}
using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Messages.Events.Timing;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLaneConfiguration : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public OpenPracticeLaneConfiguration Configuration { get; set; } = null!;
    [Parameter]
    [EditorRequired]
    public IEnumerable<Pilot> Pilots { get; set; } = Enumerable.Empty<Pilot>();
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }

    [Inject]
    public IPublishSubscribe PublishSubscribe { get; set; } = null!;

    private float _laneRSSIValue;
    private readonly List<Lap> _laneLaps = new List<Lap>();

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }


    public Task EnabledChanged(bool value)
    {
        if (Configuration.IsEnabled == value)
        {
            return Task.CompletedTask;
        }

        Configuration.IsEnabled = value;
        if (Configuration.IsEnabled)
        {
            return PublishSubscribe.Dispatch(new LaneEnabled(Configuration.LaneNumber));
        }
        else
        {
            return PublishSubscribe.Dispatch(new LaneDisabled(Configuration.LaneNumber));
        }
    }
    public Task GoToBand(byte? bandId)
    {
        if (Configuration.BandId == bandId)
        {
            return Task.CompletedTask;
        }

        if (Configuration.BandId != bandId)
        {
            if (bandId != null && Messages.Frequency.Band.Bands.Any(x => x.Id == bandId))
            {
                Configuration.FrequencyInMhz = Messages.Frequency.Band.Bands.First(x => x.Id == bandId).Frequencies.First().FrequencyInMhz;
            }
        }
        Configuration.BandId = bandId;
        return PublishSubscribe.Dispatch(new LaneRadioFrequencyConfigured(Configuration.LaneNumber, Configuration.BandId, Configuration.FrequencyInMhz));

    }
    public Task GoToFrequency(int value)
    {
        if (Configuration.FrequencyInMhz == value)
        {
            return Task.CompletedTask;
        }
        Configuration.FrequencyInMhz = value;
        return PublishSubscribe.Dispatch(new LaneRadioFrequencyConfigured(Configuration.LaneNumber, Configuration.BandId, Configuration.FrequencyInMhz));
    }
    public void Dispose()
    {
        PublishSubscribe.Unsubscribe(this);
    }
    private string GetBandString()
    {
        if (Messages.Frequency.Band.Bands.Any(x => x.Id == Configuration.BandId))
        {
            var band = Messages.Frequency.Band.Bands.First(x => x.Id == Configuration.BandId);

            return band.Name;
        }

        return $"Custom";
    }
    private string GetFrequencyString()
    {
        if (Messages.Frequency.Band.Bands.Any(x => x.Id == Configuration.BandId))
        {
            var band = Messages.Frequency.Band.Bands.First(x => x.Id == Configuration.BandId);

            if (band.Frequencies.Any(x => x.FrequencyInMhz == Configuration.FrequencyInMhz))
            {
                var frequency = band.Frequencies.First(x => x.FrequencyInMhz == Configuration.FrequencyInMhz);
                return frequency.Name;
            }
        }

        return $"{Configuration.FrequencyInMhz} Mhz";
    }
    private async Task SetPilot(Guid pilotId)
    {
        Configuration.PilotId = pilotId;

        await PublishSubscribe.Dispatch(new OpenPracticeLanePilotSet(SessionId, pilotId, Configuration.LaneNumber));
    }
    private string GetPilotString(Guid? pilotId)
    {
        var pilot = Pilots.FirstOrDefault(x => x.Id == pilotId);

        if (pilot == null)
        {
            return "Not selected";
        }

        return pilot.CallSign ?? $"{pilot.FirstName} {pilot.LastName}";
    }

    private Task TriggerDetection(byte split)
    {
        return PublishSubscribe.Dispatch(new ActiveTrackSplitLaneDetectionTriggered(Configuration.LaneNumber, split));
    }

}

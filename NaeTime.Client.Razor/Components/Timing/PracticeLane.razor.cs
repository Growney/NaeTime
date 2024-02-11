using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Messages.Events.Hardware;
using NaeTime.Messages.Events.Timing;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Components.Timing;
public partial class PracticeLane : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public LaneConfiguration Configuration { get; set; } = null!;
    [Inject]
    public IPublishSubscribe PublishSubscribe { get; set; } = null!;

    private float _laneRSSIValue;

    private readonly List<Lap> _laneLaps = new List<Lap>();

    protected override Task OnInitializedAsync()
    {
        PublishSubscribe.Subscribe<RssiLevelRecorded>(this, x =>
        {
            if (x.Lane == Configuration.LaneNumber)
            {
                _laneRSSIValue = x.Level;
                InvokeAsync(StateHasChanged);
            }
            return Task.CompletedTask;
        });

        PublishSubscribe.Subscribe<LapStarted>(this, x =>
        {
            if (x.Lane != Configuration.LaneNumber)
            {
                return Task.CompletedTask;
            }

            _laneLaps.Add(new Lap()
            {
                LapNumber = x.LapNumber,
                Lane = x.Lane,
                Started = x.StartedUtcTime,
                Status = LapStatus.Started
            });
            return Task.CompletedTask;
        });

        PublishSubscribe.Subscribe<LapCompleted>(this, x =>
        {
            if (x.Lane != Configuration.LaneNumber)
            {
                return Task.CompletedTask;
            }

            var lap = _laneLaps.FirstOrDefault(y => y.LapNumber == x.LapNumber);

            if (lap != null)
            {
                lap.Ended = x.FinishedUtcTime;
                lap.Status = LapStatus.Finished;
                lap.TotalTime = x.TotalTime;
            }
            return Task.CompletedTask;
        });

        PublishSubscribe.Subscribe<LapInvalidated>(this, x =>
        {
            if (x.Lane != Configuration.LaneNumber)
            {
                return Task.CompletedTask;
            }

            var lap = _laneLaps.FirstOrDefault(y => y.LapNumber == x.LapNumber);

            if (lap != null)
            {
                lap.Ended = x.UtcTime;
                lap.Status = LapStatus.Invalid;
                lap.TotalTime = x.TotalTime;
            }
            return Task.CompletedTask;
        });

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

    private Task TriggerDetection(byte split)
    {
        return PublishSubscribe.Dispatch(new ActiveTrackSplitLaneDetectionTriggered(Configuration.LaneNumber, split));
    }

}

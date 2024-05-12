using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Hardware.Frequency;
using NaeTime.Hardware.Messages.Messages;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLaneConfiguration : ComponentBase, IDisposable
{
    [Parameter]
    public IEnumerable<Pilot> Pilots { get; set; } = Enumerable.Empty<Pilot>();
    [Parameter]
    [EditorRequired]
    public OpenPracticeLaneConfiguration Configuration { get; set; } = null!;
    [Parameter]
    [EditorRequired]
    public Guid SessionId { get; set; }
    [Parameter]
    public Func<bool, Task>? OnEnabledChanged { get; set; }
    [Parameter]
    public Func<Guid, Task>? OnPilotChanged { get; set; }
    [Parameter]
    public Func<byte, Task>? OnDetectionTriggered { get; set; }
    [Parameter]
    public Func<byte, Task>? OnInvalidateTriggered { get; set; }
    [Parameter]
    public bool IsCollapsed { get; set; }
    [Parameter]
    public long? MaximumLapMilliseconds { get; set; }
    [Inject]
    private IEventClient EventClient { get; set; } = null!;

    [Inject]
    private IEventRegistrar EventRegistrar { get; set; } = null!;

    private IEventRegistrarScope? _registrarScope;

    protected override Task OnInitializedAsync()
    {
        _registrarScope = EventRegistrar.CreateScope();
        _registrarScope.RegisterHub(this);

        return base.OnInitializedAsync();
    }

    public async Task When(RssiLevelRecorded rssiLevelRecorded)
    {
        if (rssiLevelRecorded.Lane != Configuration.LaneNumber)
        {
            return;
        }

        Configuration.RssiValue = rssiLevelRecorded.Level;
        if (Configuration.MaxRssiValue < rssiLevelRecorded.Level)
        {
            Configuration.MaxRssiValue = rssiLevelRecorded.Level;
        }
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(LapStarted started)
    {
        if (SessionId != started.SessionId)
        {
            return;
        }

        if (started.Lane != Configuration.LaneNumber)
        {
            return;
        }

        Configuration.LapStarted = started.StartedUtcTime;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }
    public async Task When(LapInvalidated invalidated)
    {
        if (SessionId != invalidated.SessionId)
        {
            return;
        }

        if (invalidated.Lane != Configuration.LaneNumber)
        {
            return;
        }

        Configuration.LapStarted = null;

        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    public Task EnabledChanged(bool value)
    {
        if (Configuration.IsEnabled == value)
        {
            return Task.CompletedTask;
        }
        Configuration.IsEnabled = value;
        if (value)
        {
            return EventClient.Publish(new LaneEnabled(Configuration.LaneNumber));
        }
        else
        {
            return EventClient.Publish(new LaneDisabled(Configuration.LaneNumber));
        }
    }
    public Task GoToBand(byte? bandId)
    {
        int newFrequency = Configuration.FrequencyInMhz;

        if (Band.Bands.Any(x => x.Id == bandId))
        {
            var band = Band.Bands.First(x => x.Id == bandId);

            if (band.Frequencies.Any())
            {
                var firstFrequency = band.Frequencies.First();
                newFrequency = firstFrequency.FrequencyInMhz;
            }
        }
        return ChangeFrequency(bandId, newFrequency);

    }
    public Task GoToFrequency(int value) => ChangeFrequency(Configuration.BandId, value);

    private Task ChangeFrequency(byte? bandId, int frequencyInMhz)
    {
        if (Configuration.BandId == bandId && Configuration.FrequencyInMhz == frequencyInMhz)
        {
            return Task.CompletedTask;
        }

        Configuration.BandId = bandId;
        Configuration.FrequencyInMhz = frequencyInMhz;
        return EventClient.Publish(new LaneRadioFrequencyConfigured(Configuration.LaneNumber, bandId, frequencyInMhz));
    }
    private string GetBandString()
    {
        if (Band.Bands.Any(x => x.Id == Configuration.BandId))
        {
            var band = Band.Bands.First(x => x.Id == Configuration.BandId);

            return band.ShortName;
        }

        return $"Custom";
    }
    private string GetFrequencyString()
    {
        if (Band.Bands.Any(x => x.Id == Configuration.BandId))
        {
            var band = Band.Bands.First(x => x.Id == Configuration.BandId);

            if (band.Frequencies.Any(x => x.FrequencyInMhz == Configuration.FrequencyInMhz))
            {
                var frequency = band.Frequencies.First(x => x.FrequencyInMhz == Configuration.FrequencyInMhz);
                return frequency.Name;
            }
        }

        return $"{Configuration.FrequencyInMhz} Mhz";
    }
    private Task SetPilot(Guid pilotId)
    {
        if (Configuration.PilotId == pilotId)
        {
            return Task.CompletedTask;
        }
        Configuration.PilotId = pilotId;
        return EventClient.Publish(new OpenPracticeLanePilotSet(SessionId, pilotId, Configuration.LaneNumber));
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
    private Task TriggerDetection(Guid timerId) => EventClient.Publish(new OpenPracticeSessionDetectionTriggered(SessionId, Configuration.LaneNumber, timerId));

    private Task TriggerInvalidation(Guid timerId) => EventClient.Publish(new OpenPracticeSessionInvalidationTriggered(SessionId, Configuration.LaneNumber));
    public void Dispose() => _registrarScope?.Dispose();
}

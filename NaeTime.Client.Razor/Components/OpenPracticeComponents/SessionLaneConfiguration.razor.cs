using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Hardware.Frequency;
using NaeTime.Hardware.Messages;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;
using Syncfusion.Blazor.SplitButtons;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLaneConfiguration : ComponentBase, IDisposable
{
    [Parameter]
    public IEnumerable<Pilot> Pilots { get; set; } = Enumerable.Empty<Pilot>();
    [Parameter]
    [EditorRequired]
    public Lib.Models.OpenPractice.OpenPracticeLaneConfiguration Configuration { get; set; } = null!;
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
    private INaeTimeOrchestrator Orchestrator { get; set; } = null!;

    [Inject]
    private IEventRegistrarScope RegistrarScope { get; set; } = null!;

    private readonly List<RssiLevelRecorded> _rssiValues = new();

    protected override Task OnInitializedAsync()
    {
        RegistrarScope.RegisterHub(this);

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

        _rssiValues.Add(rssiLevelRecorded);

        if (_rssiValues[^1].HardwareTime - _rssiValues[0].HardwareTime > 5000)
        {
            _rssiValues.RemoveAt(0);
        }
    }

    public async Task EnabledSwitchChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args) => await EnabledChanged(args.Checked);

    public async Task EnabledChanged(bool value)
    {
        if (Configuration.IsEnabled == value)
        {
            return;
        }

        Configuration.IsEnabled = value;

        await Orchestrator.Hardware.ConfigureLaneStatus(Configuration.LaneNumber, value);
        await Orchestrator.CommitAsync();
    }
    private async Task BandSelected(MenuEventArgs x)
    {
        if (x.Item == null)
        {
            return;
        }

        if (byte.TryParse(x.Item.Id, out byte bandId))
        {
            await GoToBand(bandId);
        }
    }
    public Task GoToBand(byte? bandId)
    {
        int newFrequency = Configuration.FrequencyInMhz;

        if (Band.Bands.Any(x => x.Id == bandId))
        {
            Band band = Band.Bands.First(x => x.Id == bandId);

            if (band.Frequencies.Any())
            {
                BandFrequency firstFrequency = band.Frequencies.First();
                newFrequency = firstFrequency.FrequencyInMhz;
            }
        }

        return ChangeFrequency(bandId, newFrequency);

    }
    private async Task FrequencySelected(MenuEventArgs x)
    {
        if (x.Item == null)
        {
            return;
        }

        if (int.TryParse(x.Item.Id, out int bandId))
        {
            await GoToFrequency(bandId);
        }
    }
    public Task GoToFrequency(int value) => ChangeFrequency(Configuration.BandId, value);

    private async Task ChangeFrequency(byte? bandId, int frequencyInMhz)
    {
        if (Configuration.BandId == bandId && Configuration.FrequencyInMhz == frequencyInMhz)
        {
            return;
        }

        Configuration.BandId = bandId;
        Configuration.FrequencyInMhz = frequencyInMhz;

        await Orchestrator.OpenPractice.ConfigureLaneRadioFrequency(SessionId, Configuration.LaneNumber, bandId, frequencyInMhz);
        await Orchestrator.CommitAsync();
    }
    private string GetBandString()
    {
        if (Band.Bands.Any(x => x.Id == Configuration.BandId))
        {
            Band band = Band.Bands.First(x => x.Id == Configuration.BandId);

            return band.ShortName;
        }

        return $"Custom";
    }
    private string GetFrequencyString()
    {
        if (Band.Bands.Any(x => x.Id == Configuration.BandId))
        {
            Band band = Band.Bands.First(x => x.Id == Configuration.BandId);

            if (band.Frequencies.Any(x => x.FrequencyInMhz == Configuration.FrequencyInMhz))
            {
                BandFrequency frequency = band.Frequencies.First(x => x.FrequencyInMhz == Configuration.FrequencyInMhz);
                return frequency.Name;
            }
        }

        return $"{Configuration.FrequencyInMhz} Mhz";
    }
    private async Task PilotSelected(MenuEventArgs x)
    {
        if (x.Item == null)
        {
            return;
        }

        if (Guid.TryParse(x.Item.Id, out Guid pilotId))
        {
            await SetPilot(pilotId);
        }
    }
    private async Task SetPilot(Guid pilotId)
    {
        if (Configuration.PilotId == pilotId)
        {
            return;
        }

        Configuration.PilotId = pilotId;

        await Orchestrator.OpenPractice.ConfigureLanePilot(SessionId, Configuration.LaneNumber, pilotId);
        await Orchestrator.CommitAsync();
    }
    private string GetPilotString(Guid? pilotId)
    {
        Pilot? pilot = Pilots.FirstOrDefault(x => x.Id == pilotId);

        if (pilot == null)
        {
            return "Not selected";
        }

        return pilot.CallSign ?? $"{pilot.FirstName} {pilot.LastName}";
    }
    private Task TriggerDetection(Guid timerId) => EventClient.PublishAsync(new OpenPracticeSessionDetectionTriggered(SessionId, Configuration.LaneNumber, timerId));

    private Task TriggerInvalidation(Guid timerId) => EventClient.PublishAsync(new OpenPracticeSessionInvalidationTriggered(SessionId, Configuration.LaneNumber));
    public void Dispose() => RegistrarScope?.Dispose();
}

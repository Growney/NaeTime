using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.Models.OpenPractice;
using NaeTime.Command.Abstractions;
using NaeTime.Hardware.Frequency;
using NaeTime.Hardware.Messages;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLaneConfiguration : ComponentBase
{
    [Inject]
    private IOpenPracticeCommandHandler OpenPracticeCommandHandler { get; set; } = null!;

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

    private readonly List<RssiLevelRecorded> _rssiValues = new();

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

    public Task EnabledChanged(bool value)
    {
        if (Configuration.IsEnabled == value)
        {
            return Task.CompletedTask;
        }

        Configuration.IsEnabled = value;

        if (value)
        {
            return OpenPracticeCommandHandler.EnableLane(SessionId, Configuration.LaneNumber);
        }
        else
        {
            return OpenPracticeCommandHandler.DisableLane(SessionId, Configuration.LaneNumber);
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
    public Task GoToFrequency(int value) => ChangeFrequency(Configuration.BandId, value);

    private Task ChangeFrequency(byte? bandId, int frequencyInMhz)
    {
        if (Configuration.BandId == bandId && Configuration.FrequencyInMhz == frequencyInMhz)
        {
            return Task.CompletedTask;
        }

        Configuration.BandId = bandId;
        Configuration.FrequencyInMhz = frequencyInMhz;

        return OpenPracticeCommandHandler.TuneLane(SessionId, Configuration.LaneNumber, bandId, frequencyInMhz);
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
    private Task SetPilot(Guid pilotId)
    {
        if (Configuration.PilotId == pilotId)
        {
            return Task.CompletedTask;
        }

        Configuration.PilotId = pilotId;

        return OpenPracticeCommandHandler.SetLanePilot(SessionId, Configuration.LaneNumber, pilotId);
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
    private Task TriggerDetection(Guid timerId) => Task.CompletedTask;

    private Task TriggerInvalidation(Guid timerId) => Task.CompletedTask;
}

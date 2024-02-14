using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Components.OpenPracticeComponents;
public partial class SessionLaneConfiguration : ComponentBase
{
    [Parameter]
    public IEnumerable<Pilot> Pilots { get; set; } = Enumerable.Empty<Pilot>();
    [Parameter]
    public byte LaneNumber { get; set; }
    [Parameter]
    public Guid? PilotId { get; set; }
    [Parameter]
    public float RssiValue { get; set; }
    [Parameter]
    public float MaxRSSIValue { get; set; }
    [Parameter]
    public byte? BandId { get; set; }
    [Parameter]
    public int FrequencyInMhz { get; set; }
    [Parameter]
    public bool IsEnabled { get; set; }
    [Parameter]
    public DateTime? LapStart { get; set; }

    [Parameter]
    public Func<byte?, int, Task>? OnFrequencyChanged { get; set; }
    [Parameter]
    public Func<bool, Task>? OnEnabledChanged { get; set; }
    [Parameter]
    public Func<Guid, Task>? OnPilotChanged { get; set; }
    [Parameter]
    public Func<byte, Task>? OnDetectionTriggered { get; set; }
    [Parameter]
    public Func<byte, Task>? OnInvalidateTriggered { get; set; }


    public Task EnabledChanged(bool value)
    {
        IsEnabled = value;
        return OnEnabledChanged?.Invoke(value) ?? Task.CompletedTask;
    }
    public Task GoToBand(byte? bandId)
    {
        if (bandId == BandId)
        {
            return Task.CompletedTask;
        }
        BandId = bandId;
        if (Messages.Frequency.Band.Bands.Any(x => x.Id == BandId))
        {
            var band = Messages.Frequency.Band.Bands.First(x => x.Id == BandId);

            if (band.Frequencies.Any())
            {
                var firstFrequency = band.Frequencies.First();
                FrequencyInMhz = firstFrequency.FrequencyInMhz;
            }
        }
        return OnFrequencyChanged?.Invoke(bandId, FrequencyInMhz) ?? Task.CompletedTask;
    }
    public Task GoToFrequency(int value)
    {
        FrequencyInMhz = value;
        return OnFrequencyChanged?.Invoke(BandId, value) ?? Task.CompletedTask;
    }
    private string GetBandString()
    {
        if (Messages.Frequency.Band.Bands.Any(x => x.Id == BandId))
        {
            var band = Messages.Frequency.Band.Bands.First(x => x.Id == BandId);

            return band.ShortName;
        }

        return $"Custom";
    }
    private string GetFrequencyString()
    {
        if (Messages.Frequency.Band.Bands.Any(x => x.Id == BandId))
        {
            var band = Messages.Frequency.Band.Bands.First(x => x.Id == BandId);

            if (band.Frequencies.Any(x => x.FrequencyInMhz == FrequencyInMhz))
            {
                var frequency = band.Frequencies.First(x => x.FrequencyInMhz == FrequencyInMhz);
                return frequency.Name;
            }
        }

        return $"{FrequencyInMhz} Mhz";
    }
    private Task SetPilot(Guid pilotId)
    {
        PilotId = pilotId;
        return OnPilotChanged?.Invoke(pilotId) ?? Task.CompletedTask;
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
    private Task TriggerDetection(byte split) => OnDetectionTriggered?.Invoke(split) ?? Task.CompletedTask;
    private Task TriggerInvalidation(byte split) => OnInvalidateTriggered?.Invoke(split) ?? Task.CompletedTask;

}

using Microsoft.AspNetCore.Components;
using NaeTime.Client.Configuration.Abstractions;
using NaeTime.Client.Configuration.Models;

namespace NaeTime.Client.Razor.Components;
public partial class LocalVolume : ComponentBase
{
    private bool _isMuted;
    private int _volume;

    [Inject]
    public ILocalConfigurationRepository LocalConfiguration { get; set; } = null!;


    protected override async Task OnInitializedAsync()
    {
        SoundConfiguration currentConfiguration = await LocalConfiguration.GetSoundConfigurationAsync();
        if (currentConfiguration != null)
        {
            _isMuted = currentConfiguration.IsMuted;
            _volume = (int)(currentConfiguration.Volume * 100);
        }
        else
        {
            _isMuted = false;
            _volume = 100;
        }
        await base.OnInitializedAsync();
    }

    public async Task ToggleMute()
    {
        _isMuted = !_isMuted;
        await LocalConfiguration.SetSoundConfigurationAsync(new SoundConfiguration
        {
            IsMuted = _isMuted,
            Volume = _volume / 100.0f
        });
    }
    public async Task VolumeChange(int change)
    {
        _volume = change;
        await LocalConfiguration.SetSoundConfigurationAsync(new SoundConfiguration
        {
            IsMuted = _isMuted,
            Volume = _volume / 100.0f
        })
        ;
    }
}

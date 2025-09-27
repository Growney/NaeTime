using Microsoft.AspNetCore.Components;
using NaeTime.Client.Configuration.Models;

namespace NaeTime.Client.Razor.Components;
public partial class LocalVolume : ComponentBase
{
    private bool _isMuted;
    private int _volume;



    protected override async Task OnInitializedAsync()
    {
        SoundConfiguration currentConfiguration = new();
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
    }
    public async Task VolumeChange(int change)
    {
        _volume = change;
    }
}

using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Persistence.Client.Abstractions;

namespace NaeTime.Persistence.Client;
public class LocalConfigurationRepository : ILocalConfigurationRepository
{
    private readonly ISimpleStorageProvider _simpleStorageProvider;

    public LocalConfigurationRepository(ISimpleStorageProvider simpleStorageProvider)
    {
        _simpleStorageProvider = simpleStorageProvider ?? throw new ArgumentNullException(nameof(simpleStorageProvider));
    }

    public async Task<SoundConfiguration> GetSoundConfigurationAsync()
    {
        var volumeString = await _simpleStorageProvider.GetAsync("Volume").ConfigureAwait(false);
        var isMutedString = await _simpleStorageProvider.GetAsync("IsMuted").ConfigureAwait(false);

        if (!float.TryParse(volumeString, out var volume))
        {
            volume = 1.0f;
        }

        if (!bool.TryParse(isMutedString, out var isMuted))
        {
            isMuted = false;
        }

        return new SoundConfiguration
        {
            Volume = volume,
            IsMuted = isMuted
        };
    }

    public async Task SetSoundConfigurationAsync(SoundConfiguration soundConfiguration)
    {
        await _simpleStorageProvider.SetAsync("Volume", soundConfiguration.Volume.ToString());
        await _simpleStorageProvider.SetAsync("IsMuted", soundConfiguration.IsMuted.ToString());
    }
}


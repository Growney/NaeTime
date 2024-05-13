using NaeTime.Client.Configuration.Abstractions;
using NaeTime.Client.Configuration.Models;

namespace NaeTime.Client.Configuration;
public class SimpleStorageConfigurationRepository : ILocalConfigurationRepository
{
    private readonly ISimpleStorageProvider _simpleStorageProvider;

    public SimpleStorageConfigurationRepository(ISimpleStorageProvider simpleStorageProvider)
    {
        _simpleStorageProvider = simpleStorageProvider ?? throw new ArgumentNullException(nameof(simpleStorageProvider));
    }

    public async Task<SoundConfiguration> GetSoundConfigurationAsync()
    {
        string? volumeString = await _simpleStorageProvider.GetAsync("Volume").ConfigureAwait(false);
        string? isMutedString = await _simpleStorageProvider.GetAsync("IsMuted").ConfigureAwait(false);

        if (!float.TryParse(volumeString, out float volume))
        {
            volume = 1.0f;
        }

        if (!bool.TryParse(isMutedString, out bool isMuted))
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


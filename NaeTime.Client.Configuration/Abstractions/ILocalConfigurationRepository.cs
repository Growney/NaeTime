using NaeTime.Client.Configuration.Models;

namespace NaeTime.Client.Configuration.Abstractions;
public interface ILocalConfigurationRepository
{
    public Task<SoundConfiguration> GetSoundConfigurationAsync();
    public Task SetSoundConfigurationAsync(SoundConfiguration soundConfiguration);
}

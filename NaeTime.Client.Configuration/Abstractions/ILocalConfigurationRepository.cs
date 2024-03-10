using NaeTime.Client.Configuration.Models;

namespace NaeTime.Persistence.Client.Abstractions;
public interface ILocalConfigurationRepository
{
    public Task<SoundConfiguration> GetSoundConfigurationAsync();
    public Task SetSoundConfigurationAsync(SoundConfiguration soundConfiguration);
}

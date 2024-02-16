using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Persistence.Client.Abstractions;
public interface ILocalConfigurationRepository
{
    public Task<SoundConfiguration> GetSoundConfigurationAsync();
    public Task SetSoundConfigurationAsync(SoundConfiguration soundConfiguration);
}

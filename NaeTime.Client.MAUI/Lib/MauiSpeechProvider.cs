using NaeTime.Announcer.Abstractions;
using NaeTime.Persistence.Client.Abstractions;

namespace NaeTime.Client.MAUI.Lib;
public class MauiSpeechProvider : ISpeechProvider
{
    private readonly ILocalConfigurationRepository _localConfigurationRepository;

    public MauiSpeechProvider(ILocalConfigurationRepository localConfigurationRepository)
    {
        _localConfigurationRepository = localConfigurationRepository;
    }

    public async Task SpeakAsync(string text)
    {
        var soundConfiguration = await _localConfigurationRepository.GetSoundConfigurationAsync().ConfigureAwait(false);

        bool isMuted = false;
        float volume = 1.0f;

        if (soundConfiguration != null)
        {
            isMuted = soundConfiguration.IsMuted;
            volume = soundConfiguration.Volume;
        }

        if (!isMuted)
        {
            await TextToSpeech.SpeakAsync(text, new SpeechOptions()
            {
                Volume = volume,
            });
        }
    }

}

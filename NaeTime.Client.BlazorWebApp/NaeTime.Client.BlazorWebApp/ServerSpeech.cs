
using EventDbLite.Abstractions;
using EventDbLite.Reactions.Abstractions;
using NaeTime.Announcer;
using System.Speech.Synthesis;

namespace NaeTime.Client.BlazorWebApp;

public class ServerSpeech : BackgroundService
{
    private IServiceProvider _serviceProvider;

    public ServerSpeech(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        IReactionClassFactory factory = scope.ServiceProvider.GetRequiredService<IReactionClassFactory>();

        using IReactionClassContainer<AnnouncerReactions> container = factory.Create<AnnouncerReactions>();

        // Initialize a new instance of the SpeechSynthesizer.
        SpeechSynthesizer synth = new ();

        // Configure the audio output.
        synth.SetOutputToDefaultAudioDevice();

        await foreach(var message in container.Instance.WithCancellation(stoppingToken))
        {
            synth.Speak(message);
        }

    }
}

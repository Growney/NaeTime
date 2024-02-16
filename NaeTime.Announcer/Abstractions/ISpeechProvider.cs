namespace NaeTime.Announcer.Abstractions;
public interface ISpeechProvider
{
    public Task SpeakAsync(string text);
}

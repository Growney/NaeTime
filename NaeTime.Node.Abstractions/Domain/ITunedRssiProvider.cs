namespace NaeTime.Node.Abstractions.Domain;

public interface ITunedRssiProvider
{
    int CurrentFrequency { get; }
    Task<bool> Tune(int frequency);
    int GetRssi();
}

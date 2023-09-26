namespace NaeTime.Node.Abstractions.Models;

public struct TuningResult
{
    public TuningResult(int requestedFrequency, int tunedFrequency, bool success)
    {
        RequestedFrequency = requestedFrequency;
        TunedFrequency = tunedFrequency;
        Success = success;
    }

    public int RequestedFrequency { get; }
    public int TunedFrequency { get; }
    public bool Success { get; }
}

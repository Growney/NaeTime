using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;
using RX5808;

namespace NaeTime.Node.Domain;

public class MultiplexedRx5808TunningChannel : ITuningCommunication
{
    public int TunedFrequency { get; private set; }
    public int ActualFrequency { get; private set; }

    private readonly MultiplexedRx5808Group _multiplexerGroup;
    private readonly byte _channel;

    public MultiplexedRx5808TunningChannel(MultiplexedRx5808Group multiplexerGroup, byte channel)
    {
        _multiplexerGroup = multiplexerGroup ?? throw new ArgumentNullException(nameof(multiplexerGroup));
        _channel = channel;
    }

    public async Task<TuningResult> TuneDeviceAsync(int frequency)
    {
        await _multiplexerGroup.SetFrequencyAsync(_channel, frequency);

        var storedFrequency = await _multiplexerGroup.GetActualStoredFrequencyAsync(_channel);
        var isSuccess = RX5808RegisterCommunication.DoTunedFrequenciesMatch(frequency, storedFrequency);

        TunedFrequency = frequency;
        ActualFrequency = storedFrequency;

        return new TuningResult(frequency, storedFrequency, isSuccess);
    }
}

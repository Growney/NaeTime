using RX5808;

namespace NaeTime.Node.Domain;

public class MultiplexedRx5808Group
{
    private readonly MultiplexedRX5808RegisterCommunication _communication;

    public MultiplexedRx5808Group(IRX5808Factory moduleFactory, int multiplexerId, byte aAddressPin, byte bAddressPin, byte cAddressPin, byte dataPin, byte selectPin, byte clockPin)
    {
        MultiplexerId = multiplexerId;

        AAddressPin = aAddressPin;
        BAddressPin = bAddressPin;
        CAddressPin = cAddressPin;

        DataPin = dataPin;
        SelectPin = selectPin;
        ClockPin = clockPin;

        _communication = moduleFactory.GetMultiplexedModule(aAddressPin, bAddressPin, cAddressPin, dataPin, selectPin, clockPin);
    }

    public int MultiplexerId { get; }
    public byte AAddressPin { get; }
    public byte BAddressPin { get; }
    public byte CAddressPin { get; }
    public byte DataPin { get; }
    public byte SelectPin { get; }
    public byte ClockPin { get; }

    public Task SetFrequencyAsync(byte channel, int frequencyInMhz)
        => _communication.PerformAction(channel, x => x.SetFrequencyAsync(frequencyInMhz));
    public Task<bool> ConfirmFrequencyAsync(byte channel, int frequencyInMhz)
        => _communication.GetResult(channel, x => x.ConfirmFrequencyAsync(frequencyInMhz));
    public Task<int> GetActualStoredFrequencyAsync(byte channel)
        => _communication.GetResult(channel, x => x.GetActualStoredFrequencyAsync());
}

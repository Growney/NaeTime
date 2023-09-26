using NaeTime.Node.Abstractions.Domain;

namespace NaeTime.Node.Domain;

public class Rx5808Device : ITunedRssiDevice
{
    public Rx5808Device(byte deviceId, IRssiCommunication rssiCommunication, ITuningCommunication tunningCommunication, bool isEnabled)
    {
        DeviceId = deviceId;
        RssiCommunication = rssiCommunication ?? throw new ArgumentNullException(nameof(rssiCommunication));
        TunningCommunication = tunningCommunication ?? throw new ArgumentNullException(nameof(tunningCommunication));
        IsEnabled = isEnabled;
    }

    public byte DeviceId { get; }
    public IRssiCommunication RssiCommunication { get; }
    public ITuningCommunication TunningCommunication { get; }
    public bool IsEnabled { get; set; }
}

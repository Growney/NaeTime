namespace NaeTime.Node.Abstractions.Domain;

public interface ITunedRssiDevice
{
    byte DeviceId { get; }
    bool IsEnabled { get; set; }
    IRssiCommunication RssiCommunication { get; }
    ITuningCommunication TunningCommunication { get; }
}

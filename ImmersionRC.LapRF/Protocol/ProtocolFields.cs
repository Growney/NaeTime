namespace ImmersionRC.LapRF.Protocol;
internal enum ProtocolFields
{
    TransponderId = 0x01,
    Timestamp = 0x02,
    PassNumber = 0x21,
}
internal enum StatusField
{
    TransponderId = 0x01,
    Timestamp = 0x02,
    StatusFlags = 0x03,
    InputVoltage = 0x21,
    ReceivedSignalStrengthIndicator = 0x22,
    GateState = 0x23,
    StatusCount = 0x24
}
internal enum RadioFrequencySetupField
{
    TransponderId = 0x01,
    IsEnabled = 0x20,
    Channel = 0x21,
    Band = 0x22,
    Threshold = 0x23,
    Attenuation = 0x24,
    Frequency = 0x25,
}
namespace NaeTime.Query.Abstractions.Models;
public enum DetectorType
{
    ImmersionRC = 0b0000_0000_0000_0001,
    EthernetLapRF8Channel = 0b0000_0000_0000_0010 | ImmersionRC,

    NaeTime = 0b0000_0001_0000_0000,
    NaeTimeSerial = 0b0000_0010_0000_0000 | NaeTime,
}

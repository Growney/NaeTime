namespace ELRS.Backpack;

internal enum BackpackCommands
{
    GetBandChannelIndex = 0x0300,
    SetBandChannelIndex = 0x0301,
    GetFrequency = 0x0302,
    SetFrequency = 0x0303,
    GetRecordingState = 0x0304,
    SetRecordingState = 0x0305,
    GetVRxMode = 0x0306,
    SetVRxMode = 0x0307,
    GetRSSI = 0x0308,
    GetBatteryVoltage = 0x0309,
    GetVRxVersion = 0x030A,
    SetBuzzer = 0x030B,
    SetHeadTrackingState = 0x030D,
    SetOSDElement = 0x00B6,
    SetMode = 0x0380,
    GetBackpackVersion = 0x0381,
    GetStatus = 0x0382,
    SetHeadTrackingData = 0x0383,

    // ELRS-specific opcodes
    GetRfMode = 0x0006,
    SetTxPower = 0x0007,
    GetTelemetryRate = 0x0008,
    Bind = 0x0009,
    GetModelId = 0x000A,
    RequestVtxPacket = 0x000B,
    SetTxBackpackWifiMode = 0x000C,
    SetVrxBackpackWifiMode = 0x000D,
    SetRxWifiMode = 0x000E,
    SetRxLoanMode = 0x000F,
    GetBackpackVersionElrs = 0x0010,
    GetBackpackCrsfTelemetry = 0x0011,
    SetUid = 0x00B5,

    // ELRS backpack protocol opcodes (outgoing)
    GetBackpackFirmware = 0x030A,
    SetBackpackOsdElement = 0x030C,
    SetBackpackRtc = 0x030E,

    // ELRS backpack protocol opcodes (incoming)
    SetBackpackPointer = 0x0383
}
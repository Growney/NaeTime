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
    SetHeadTrackingData = 0x0383
}
namespace ELRS.Backpack;

public enum DisplayportCommand
{
    Heartbeat = 0x00,
    ReleaseThePort = 0x01,
    ClearScreen = 0x02,
    WriteString = 0x03,
    DrawScreen = 0x04,
}

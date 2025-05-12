namespace ELRS.Backpack;

public class BackpackCommand
{
    public enum HeaderLocation : byte
    {
        Start = 0,
        Version = 1,
        Type = 2,
        Flag = 3,
        Function = 4,
        PayloadSize = 6
    }
    public const int HeaderSize = (byte)HeaderLocation.PayloadSize + 2;
    public const int CrcSize = 1;
    public const char StartingCharacter = '$';
    public const char Version = 'X';

    public CommandType Type { get; init; } = CommandType.Request;
    public byte Flag { get; init; } = 0;
    public BackpackCommands Function { get; init; }
    public byte[] Payload { get; init; } = Array.Empty<byte>();
    public ushort PayloadSize => (ushort)Payload.Length;
    public bool ShouldAwaitResponse { get; init; } = false;
}

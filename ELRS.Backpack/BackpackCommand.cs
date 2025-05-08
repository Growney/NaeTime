namespace ELRS.Backpack;

internal class BackpackCommand
{
    public byte StartingCharacter { get; init; }
    public byte Version { get; init; } = (byte)'X';
    public CommandType Type { get; init; } = CommandType.Request;
    public byte Flag { get; init; } = 0;
    public BackpackCommands Function { get; init; }
    public byte[] Payload { get; init; } = Array.Empty<byte>();
    public ushort PayloadSize => (ushort)Payload.Length;
    public bool ShouldAwaitResponse { get; init; } = false;


}

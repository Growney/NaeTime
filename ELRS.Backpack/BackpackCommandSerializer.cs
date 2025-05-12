namespace ELRS.Backpack;

internal class BackpackCommandSerializer : IBackpackCommandSerializer
{
    public byte[] Serialize(BackpackCommand command)
    {
        using MemoryStream crcMemoryStream = new();
        using BinaryWriter crcWriter = new(crcMemoryStream);
        crcWriter.Write(command.Flag);
        crcWriter.Write((ushort)command.Function);
        crcWriter.Write(command.PayloadSize);
        crcWriter.Write(command.Payload);

        byte[] toBeCRC = crcMemoryStream.ToArray();

        Crc8 crc = new();
        byte crcValue = crc.ComputeChecksum(toBeCRC);

        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(BackpackCommand.StartingCharacter);
        writer.Write(BackpackCommand.Version);
        writer.Write((byte)command.Type);
        writer.Write(command.Flag);
        writer.Write((ushort)command.Function);
        writer.Write(command.PayloadSize);
        writer.Write(command.Payload);
        writer.Write(crcValue);

        return memoryStream.ToArray();
    }
}

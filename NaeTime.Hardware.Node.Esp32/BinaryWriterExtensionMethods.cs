using NaeTime.Hardware.Node.Esp32;

namespace System.IO;
public static class BinaryWriterExtensionMethods
{
    public static void WriteRecordType(this BinaryWriter writer, RecordType recordType)
        => writer.WriteRecordType((byte)recordType);
    public static void WriteRecordType(this BinaryWriter writer, byte recordType)
    {
        RecordHeader header = new(0, 0, recordType);
        RecordHeader.Write(writer, header);
    }
    public static byte[] FinalisePacketData(this MemoryStream stream)
    {
        CRC16 crc16 = new();
        byte[] data = stream.ToArray();
        RecordHeader.SetRecordLength(data, (ushort)data.Length);
        ushort crc = crc16.Calculate(data);
        RecordHeader.SetRecordCRC(data, crc);
        byte[] escapedData = NodeProtocol.DataEscaper.Escape(data);

        return escapedData;
    }
}

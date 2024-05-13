namespace ImmersionRC.LapRF.Protocol;
public static class BinaryWriterExtensionMethods
{
    public static void WriteRecordType(this BinaryWriter writer, RecordType recordType)
    {
        RecordHeader header = new(0, 0, (ushort)recordType);
        RecordHeader.Write(writer, header);
    }
    public static void WriteField(this BinaryWriter writer, byte fieldId, byte field)
        => WriteField(writer, fieldId, sizeof(byte), new byte[] { field });
    public static void WriteField(this BinaryWriter writer, byte fieldId, ushort field)
        => WriteField(writer, fieldId, sizeof(ushort), BitConverter.GetBytes(field));

    private static void WriteField(this BinaryWriter writer, byte fieldId, byte size, byte[] bytes)
    {
        //Field id
        writer.Write(fieldId);
        //Field length
        writer.Write(size);
        //Field value
        writer.Write(bytes);
    }
    public static byte[] FinalisePacketData(this MemoryStream stream)
    {
        Crc16 crc16 = new();
        byte[] data = stream.ToArray();
        RecordHeader.SetRecordLength(data, (ushort)data.Length);
        ushort crc = crc16.Compute(data, data.Length);
        RecordHeader.SetRecordCRC(data, crc);
        byte[] escapedData = LapRFProtocol.DataEscaper.Escape(data);

        return escapedData;
    }
}

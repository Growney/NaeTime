namespace NaeTime.Hardware.Node.Esp32;
internal struct RecordHeader
{
    private const int HEADER_START_INDEX = 0;
    private const int TYPE_START_INDEX = HEADER_START_INDEX;
    private const int CRC_START_INDEX = TYPE_START_INDEX + 1;
    private const int LENGTH_START_INDEX = CRC_START_INDEX + 2;
    private const int DATA_START_INDEX = LENGTH_START_INDEX + 2;
    public RecordHeader(ushort recordLength, ushort recordCRC, byte recordTypeRaw)
    {
        RecordLength = recordLength;
        RecordCRC = recordCRC;
        RecordTypeRaw = recordTypeRaw;
    }

    public ushort RecordLength { get; }
    public ushort RecordCRC { get; }
    public byte RecordTypeRaw { get; }
    public RecordType RecordType => (RecordType)RecordTypeRaw;

    public static int GetDataStartLocation() => DATA_START_INDEX;
    public static RecordHeader Read(ReadOnlySpan<byte> buffer)
        => new(
            BitConverter.ToUInt16(buffer.Slice(LENGTH_START_INDEX, sizeof(ushort))),
            BitConverter.ToUInt16(buffer.Slice(CRC_START_INDEX, sizeof(ushort))),
            buffer[TYPE_START_INDEX]
            );
    public static void Write(BinaryWriter writer, RecordHeader header)
    {
        writer.Write(header.RecordTypeRaw);
        writer.Write(header.RecordCRC);
        writer.Write(header.RecordLength);
    }

    public static void SetRecordLength(byte[] buffer, ushort recordLength) => BitConverter.GetBytes(recordLength).CopyTo(buffer, LENGTH_START_INDEX);

    public static void SetRecordCRC(Span<byte> buffer, ushort crc) => BitConverter.GetBytes(crc).AsSpan().CopyTo(buffer.Slice(CRC_START_INDEX, sizeof(ushort)));
}

namespace NaeTime.Bytes;
public static class ReadOnlySpanReaderExtensions
{
    public static long ReadInt64(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(long));
        return BitConverter.ToInt64(span);
    }
    public static int ReadInt32(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(int));
        return BitConverter.ToInt32(span);
    }
    public static short ReadInt16(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(short));
        return BitConverter.ToInt16(span);
    }
    public static byte ReadByte(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(byte));
        return span[0];
    }
    public static ulong ReadUInt64(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(ulong));
        return BitConverter.ToUInt64(span);
    }
    public static uint ReadUInt32(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(uint));
        return BitConverter.ToUInt32(span);
    }
    public static ushort ReadUInt16(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(ushort));
        return BitConverter.ToUInt16(span);
    }
    public static float ReadSingle(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(float));
        return BitConverter.ToSingle(span);
    }
    public static float ReadFloat(this ref ReadOnlySpanReader<byte> reader)
    {
        ReadOnlySpan<byte> span = reader.Read(sizeof(float));
        return BitConverter.ToSingle(span);
    }
}

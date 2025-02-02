namespace NaeTime.Hardware.Node.Esp32;
public class CRC16
{
    private readonly ushort[] _table;
    private readonly ushort _polynomial;
    private readonly ushort _initialValue;

    public CRC16(ushort polynomial = 0x8005, ushort initialValue = 0xFFFF)
    {
        _polynomial = polynomial;
        _initialValue = initialValue;
        _table = CreateTable();
    }

    private ushort[] CreateTable()
    {
        ushort[] table = new ushort[256];
        for (int byteValue = 0; byteValue < 256; byteValue++)
        {
            ushort crc = 0;
            ushort currentByte = (ushort)byteValue;
            for (int bit = 0; bit < 8; bit++)
            {
                if (((crc ^ currentByte) & 0x0001) != 0)
                {
                    crc = (ushort)((crc >> 1) ^ _polynomial);
                }
                else
                {
                    crc >>= 1;
                }
                currentByte >>= 1;
            }
            table[byteValue] = crc;
        }

        return table;
    }

    private ushort Reflect(ushort data, int width)
    {
        ushort reflection = 0;
        for (int i = 0; i < width; i++)
        {
            if ((data & (1 << i)) != 0)
            {
                reflection |= (ushort)(1 << (width - 1 - i));
            }
        }

        return reflection;
    }

    public ushort Calculate(ReadOnlySpan<byte> data)
    {
        ushort crc = _initialValue;
        foreach (byte byteValue in data)
        {
            byte reflectedByte = (byte)Reflect(byteValue, 8);
            crc = (ushort)((crc >> 8) ^ _table[(crc ^ reflectedByte) & 0xFF]);
        }

        crc = Reflect(crc, 16);
        return crc;
    }
}
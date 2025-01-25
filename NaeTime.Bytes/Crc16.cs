namespace NaeTime.Bytes;
public class Crc16
{
    private readonly ushort[] _crc16_table;

    //-------------------------------------------------------------------------------------------------------
    public Crc16()
    {
        _crc16_table = InitCRC16Table();
    }

    //-------------------------------------------------------------------------------------------------------
    private ushort[] InitCRC16Table()
    {
        ushort[] table = new ushort[256];

        for (ushort i = 0; i < 256; i += 1)
        {
            ushort remainder = (ushort)(i << 8 & 0xFF00);
            for (int j = 8; j > 0; --j)
            {
                remainder = (remainder & 0x8000) == 0x8000 ? (ushort)((remainder << 1) & 0xFFFF ^ 0x8005) : (ushort)((remainder << 1) & 0xFFFF);
            }

            table[i] = remainder;
        }

        return table;
    }

    //-------------------------------------------------------------------------------------------------------
    private ushort Reflect(ushort input, int numberOfBits)
    {
        ushort output = 0;
        for (int i = 0; i < numberOfBits; i++)
        {
            if ((input & 0x01) == 0x01)
            {
                output |= (ushort)(1 << (numberOfBits - 1 - i));
            }

            input = (ushort)(input >> 1);
        }

        return output;
    }

    //-------------------------------------------------------------------------------------------------------
    public ushort Compute(byte[] dataIn, int length)
    {
        ushort remainder = 0x0000;

        for (int i = 0; i < length; i++)
        {
            ushort a = Reflect(dataIn[i], 8);
            a &= 0xff;
            ushort b = (ushort)((remainder >> 8) & 0xFF);
            ushort c = (ushort)((remainder << 8) & 0xFFFF);
            ushort data = (ushort)(a ^ b);
            remainder = (ushort)(_crc16_table[data] ^ c);
        }

        return Reflect(remainder, 16);
    }
}

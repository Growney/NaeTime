namespace ImmersionRC.LapRF.Protocol;
internal class Crc16
{
    private readonly UInt16[] _crc16_table;

    //-------------------------------------------------------------------------------------------------------
    public Crc16()
    {
        _crc16_table = InitCRC16Table();
    }

    //-------------------------------------------------------------------------------------------------------
    private UInt16[] InitCRC16Table()
    {
        UInt16 remainder = 0;
        ushort[] table = new UInt16[256];

        for (UInt16 i = 0; i < 256; i += 1)
        {
            remainder = (UInt16)((i << 8) & 0xFF00);
            for (int j = 8; j > 0; --j)
            {
                if ((remainder & (UInt16)0x8000) == (UInt16)0x8000)
                    remainder = (UInt16)(((remainder << 1) & (UInt16)0xFFFF) ^ (UInt16)0x8005);
                else
                    remainder = (UInt16)(((remainder << 1) & 0xFFFF));
            }

            table[i] = remainder;
        }

        return table;
    }

    //-------------------------------------------------------------------------------------------------------
    private UInt16 Reflect(UInt16 input, int numberOfBits)
    {
        UInt16 output = 0;
        for (int i = 0; i < numberOfBits; i++)
        {
            if ((input & (UInt16)0x01) == (UInt16)0x01)
            {
                output |= (UInt16)(1 << ((numberOfBits - 1) - i));
            }

            input = (UInt16)(input >> 1);
        }

        return output;
    }

    //-------------------------------------------------------------------------------------------------------
    public UInt16 Compute(Byte[] dataIn, int length)
    {
        UInt16 remainder = (UInt16)0x0000;

        for (int i = 0; i < length; i++)
        {
            UInt16 a = Reflect(dataIn[i], 8);
            a &= 0xff;
            UInt16 b = (UInt16)((remainder >> 8) & ((UInt16)0xFF));
            UInt16 c = (UInt16)((remainder << 8) & 0xFFFF);
            UInt16 data = (UInt16)((int)a ^ (int)b);
            remainder = (UInt16)((int)_crc16_table[data] ^ (int)c);
        }

        return Reflect(remainder, 16);
    }
}

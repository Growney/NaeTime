namespace ELRS.Backpack;

public class Crc8
{
    private const byte Polynomial = 0xD5;

    // Method to calculate CRC8-DVB-S2 for a single byte
    private byte Crc8DvbS2(byte crc, byte a)
    {
        crc ^= a;
        for (int i = 0; i < 8; i++)
        {
            if ((crc & 0x80) != 0)
            {
                crc = (byte)((crc << 1) ^ Polynomial);
            }
            else
            {
                crc <<= 1;
            }
        }
        return (byte)(crc & 0xFF);
    }

    // Method to calculate the checksum for a byte array
    public byte ComputeChecksum(byte[] body)
    {
        if (body == null)
            throw new ArgumentNullException(nameof(body));

        byte crc = 0x00; // Initial CRC value
        foreach (var b in body)
        {
            crc = Crc8DvbS2(crc, b);
        }
        return crc;
    }
}

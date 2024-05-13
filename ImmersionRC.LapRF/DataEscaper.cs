
namespace ImmersionRC.LapRF;
public class DataEscaper
{
    private readonly byte _escape;
    private readonly byte[] _escapeCharacters;
    private readonly byte _escapedAdder;
    //Number of bytes that should be ignored at the start of the data for escaping
    private readonly int _escapeStart;
    //Number of bytes that should be ignored at the end of the data for escaping
    private readonly int _escapeEnd;

    /// <summary>
    /// Creates a new instance of the <see cref="DataEscaper"/> class.
    /// </summary>
    /// <param name="escape">The escape character</param>
    /// <param name="escapedAdder">Value that should be added to characters that have been escaped and subtracted when unescaped</param>
    /// <param name="escapeStart">Number of bytes that should be ignored at the start of the data for escaping</param>
    /// <param name="escapeEnd">Number of bytes that should beb ignored at the end of the data for escaping</param>
    /// <param name="escapeCharacters">Characters that should be escaped</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DataEscaper(byte escape, byte escapedAdder, int escapeStart, int escapeEnd, params byte[] escapeCharacters)
    {
        _escape = escape;
        _escapeCharacters = escapeCharacters ?? throw new ArgumentNullException(nameof(escapeCharacters));
        _escapedAdder = escapedAdder;
        _escapeStart = escapeStart;
        _escapeEnd = escapeEnd;
    }

    public byte[] UnEscape(ReadOnlySpan<byte> data)
    {
        List<byte> unEscapedData = new(data.Length);

        int readIndex = 0;
        while (readIndex < data.Length)
        {
            byte dataByte = data[readIndex];

            if (dataByte == _escape)
            {
                readIndex++;
                unEscapedData.Add((byte)(data[readIndex] - _escapedAdder));
            }
            else
            {
                unEscapedData.Add(dataByte);
            }

            readIndex++;
        }

        return unEscapedData.ToArray();
    }

    public byte[] Escape(ReadOnlySpan<byte> data)
    {
        List<byte> escapedData = new(data.Length);

        for (int dataIndex = 0; dataIndex < data.Length; dataIndex++)
        {
            byte currentData = data[dataIndex];

            if (IsInEscapeBounds(dataIndex, data.Length) && _escapeCharacters.Contains(currentData))
            {
                escapedData.Add(_escape);
                escapedData.Add((byte)(currentData + _escapedAdder));
            }
            else
            {
                escapedData.Add(currentData);
            }
        }

        return escapedData.ToArray();
    }

    private bool IsInEscapeBounds(int index, int dataLength)
    {
        return index > _escapeStart && index < (dataLength - _escapeEnd);
    }
}

namespace ImmersionRC.LapRF;
public ref struct ReadOnlySpanReader<T>
{
    private readonly ReadOnlySpan<T> _data;
    private int _position;

    public ReadOnlySpanReader(ReadOnlySpan<T> data)
    {
        _data = data;
        _position = 0;
    }

    public ReadOnlySpan<T> Peek(int length)
    {
        return _data.Slice(_position, length);
    }
    public ReadOnlySpan<T> Read(int length)
    {
        ReadOnlySpan<T> span = _data.Slice(_position, length);
        _position += length;
        return span;
    }
    public void Reset()
    {
        _position = 0;
    }

    public bool HasData()
    {
        return _position < _data.Length;
    }
}

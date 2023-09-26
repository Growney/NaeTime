namespace NaeTime.Server.Domain;

public class OrderedStream<T>
{
    private struct OrderedItem
    {
        public OrderedItem(T value, long tick)
        {
            Value = value;
            Tick = tick;
        }

        public T Value { get; }
        public long Tick { get; }
    }
    private readonly List<OrderedItem> _buffer;
    private readonly int _threshold;

    private long? _mostUpToDateTick = null;

    public OrderedStream(int threshold = 10)
    {
        _threshold = threshold;
        _buffer = new List<OrderedItem>();
    }

    public IEnumerable<T> ProcessNext(long tick, T item)
    {
        if (_mostUpToDateTick != null && (_mostUpToDateTick - tick) > _threshold)
        {
            yield break;
        }

        var orderedItem = new OrderedItem(item, tick);

        var addIndex = _buffer.FindIndex(x => tick < x.Tick);
        if (addIndex < 0)
        {
            addIndex = _buffer.FindIndex(x => tick > x.Tick) + 1;
        }

        _buffer.Insert(addIndex, orderedItem);

        if (_buffer.Count > 1)
        {
            if (_mostUpToDateTick != null)
            {
                var firstItem = _buffer[0];
                var lastItem = _buffer[^1];
                //All items must be there and in order as there can be no duplicates
                if ((lastItem.Tick - firstItem.Tick) + 1 == _buffer.Count)
                {
                    while (_buffer.Count > 0)
                    {
                        yield return _buffer[0].Value;
                        _buffer.RemoveAt(0);
                    }
                }
                else
                {
                    while (lastItem.Tick - firstItem.Tick > _threshold)
                    {
                        yield return _buffer[0].Value;
                        _buffer.RemoveAt(0);

                        firstItem = _buffer[0];
                        lastItem = _buffer[^1];
                    }
                }
            }
        }
        else
        {
            if (_mostUpToDateTick + 1 == tick)
            {
                yield return _buffer[0].Value;
                _buffer.RemoveAt(0);
            }
        }

        if (_mostUpToDateTick == null || tick > _mostUpToDateTick)
        {
            _mostUpToDateTick = tick;
        }
    }
}

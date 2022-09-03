using NaeTime.Node.Abstractions;
using NaeTime.Node.Abstractions.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Core
{
    public class RssiStreamAggregationQueue : IRssiStreamAggregationQueue
    {
        private class StreamBuffer
        {
            public long? CurrentTick { get; set; }
            public List<RssiStreamReading> Readings { get; } = new List<RssiStreamReading>();
        }
        private readonly ConcurrentQueue<RssiStreamReading> _mainQueue = new();
        private readonly Dictionary<Guid, StreamBuffer> _streamBuffers = new();

        public bool HasValues => _mainQueue.Count() > 0;

        public IEnumerable<RssiStreamReading> Dequeue()
        {
            int countAtQuery = _mainQueue.Count;
            while (countAtQuery > 0)
            {
                if (_mainQueue.TryDequeue(out var reading))
                {
                    yield return reading;
                }
                countAtQuery--;
            }
        }

        public void Enqueue(RssiStreamReading rssiReading)
        {
            if (!_streamBuffers.TryGetValue(rssiReading.StreamId, out var streamBuffer))
            {
                streamBuffer = new StreamBuffer();
                _streamBuffers.Add(rssiReading.StreamId, streamBuffer);
            }

            if (streamBuffer.CurrentTick == null)
            {
                streamBuffer.CurrentTick = rssiReading.Tick;
            }
            else if (rssiReading.Tick != streamBuffer.CurrentTick)
            {
                var averageValue = (int)streamBuffer.Readings.Average(x => x.Value);
                var aggregateReading = new RssiStreamReading(rssiReading.StreamId, rssiReading.Tick, averageValue);
                _mainQueue.Enqueue(aggregateReading);

                streamBuffer.Readings.Clear();
                streamBuffer.CurrentTick = rssiReading.Tick;
            }

            streamBuffer.Readings.Add(rssiReading);
        }

        public void Clear()
        {
            _mainQueue.Clear();
            _streamBuffers.Clear();
        }
    }
}

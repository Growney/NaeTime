using NaeTime.Abstractions.Models;

namespace NaeTime.Core.Tests
{
    public class RssiRandomGenerator
    {
        private readonly Random _random;
        public RssiRandomGenerator(int seed)
        {
            _random = new Random(seed);
        }
        public IEnumerable<int> GenerateNoise(int count, int value, int jitter)
        {
            for (int i = 0; i < count; i++)
            {
                yield return _random.Next(value - jitter, value + jitter);
            }
        }
        public IEnumerable<RssiStreamReading> GenerateStreamNoiseReadings(Guid streamId, long startingTick, int count, int value, int jitter)
        {
            foreach (var noise in GenerateNoise(count, value, jitter))
            {
                yield return new RssiStreamReading()
                {
                    Id = Guid.NewGuid(),
                    StreamId = streamId,
                    Tick = ++startingTick,
                    Value = noise
                };
            }
        }
        public IEnumerable<int> GeneratePeak(int width, int value, int jitter)
        {
            for (int i = 0; i < width; i++)
            {
                double piValue = (Math.PI / (width - 1)) * i;
                int sinValue = (int)(Math.Sin(piValue) * value);
                yield return _random.Next(sinValue - jitter, sinValue + jitter);
            }
        }

        public IEnumerable<RssiStreamReading> GenerateStreamPeakReadings(Guid streamId, long startingTick, int width, int value, int jitter)
        {
            foreach (var noise in GeneratePeak(width, value, jitter))
            {
                yield return new RssiStreamReading()
                {
                    Id = Guid.NewGuid(),
                    StreamId = streamId,
                    Tick = ++startingTick,
                    Value = noise
                };
            }
        }
        public IEnumerable<RssiStreamReading> CreateStream(params IEnumerable<RssiStreamReading>[] components)
        {
            foreach (var component in components)
            {
                foreach (var reading in component)
                {
                    yield return reading;
                }
            }
        }

    }
}

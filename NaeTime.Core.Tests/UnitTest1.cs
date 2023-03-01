//using NaeTime.Abstractions.Models;
//using NaeTime.Core.Processors;

//namespace NaeTime.Core.Tests
//{
//    public class UnitTest1
//    {
//        private readonly RssiRandomGenerator _randomGenerator;
//        public UnitTest1()
//        {
//            _randomGenerator = new RssiRandomGenerator(123456789);
//        }
//        [Fact]
//        public void Test1()
//        {
//            var streamId = Guid.NewGuid();
//            var noisyStream = _randomGenerator.CreateStream(
//                _randomGenerator.GenerateStreamNoiseReadings(streamId, 0, 5000, 150, 10),
//                _randomGenerator.GenerateStreamPeakReadings(streamId, 5001, 100, 300, 10),
//                _randomGenerator.GenerateStreamNoiseReadings(streamId, 5101, 1000, 150, 10));

//            var nodeId = Guid.NewGuid();
//            var trackId = Guid.NewGuid();
//            var track = new Track()
//            {
//                Id = Guid.NewGuid(),
//                Gates = new List<TimedGate>()
//                {
//                    new()
//                    {
//                        Id = Guid.NewGuid(),
//                        Position = 0,
//                        NodeId = nodeId,
//                        RssiReceiverType = Abstractions.Enumeration.RssiReceiverType.RX5808,
//                        TrackId = trackId
//                    }
//                }
//            };

//            var flight = new Flight()
//            {
//                Id = Guid.NewGuid(),
//            };
//            var stream = new RssiStream()
//            {
//                Id = streamId,
//                Boundary = new RssiBoundary() { PeakStartRssi = 250, PeakEndRssi = 240 },
//                FlightId = flight.Id

//            };
//            flight.RssiStreams.Add(stream);

//            var readingProcessor = new FlightProcessor(track, flight, stream);

//            foreach (var reading in noisyStream)
//            {
//                readingProcessor.ProcessReading(reading);
//            }

//            Assert.Single(stream.RssiReadingPasses);
//        }

//        [Fact]
//        public void Test2()
//        {
//            var streamId = Guid.NewGuid();
//            var noisyStream = _randomGenerator.CreateStream(
//                _randomGenerator.GenerateStreamNoiseReadings(streamId, 0, 5000, 150, 10),
//                _randomGenerator.GenerateStreamPeakReadings(streamId, 5001, 100, 300, 10),
//                _randomGenerator.GenerateStreamNoiseReadings(streamId, 5101, 3000, 150, 10),
//                _randomGenerator.GenerateStreamPeakReadings(streamId, 5001, 100, 300, 10),
//                _randomGenerator.GenerateStreamNoiseReadings(streamId, 5101, 3000, 150, 10));

//            var nodeId = Guid.NewGuid();
//            var trackId = Guid.NewGuid();
//            var track = new Track()
//            {
//                Id = Guid.NewGuid(),
//                Gates = new List<TimedGate>()
//                {
//                    new()
//                    {
//                        Id = Guid.NewGuid(),
//                        Position = 0,
//                        NodeId = nodeId,
//                        RssiReceiverType = Abstractions.Enumeration.RssiReceiverType.RX5808,
//                        TrackId = trackId
//                    }
//                }
//            };

//            var flight = new Flight()
//            {
//                Id = Guid.NewGuid()
//            };
//            var stream = new RssiStream()
//            {
//                Id = streamId,
//                Boundary = new RssiBoundary() { PeakStartRssi = 250, PeakEndRssi = 240 },
//                FlightId = flight.Id,
//                NodeId = nodeId,

//            };
//            flight.RssiStreams.Add(stream);

//            var readingProcessor = new FlightProcessor(track, flight, stream);

//            foreach (var reading in noisyStream)
//            {
//                readingProcessor.ProcessReading(reading);
//            }

//            Assert.Equal(2, stream.RssiReadingPasses.Count());
//            //One finished lap
//            Assert.Single(flight.Laps.Where(x => x.EndTick != null));
//            //Two Total laps
//            Assert.Equal(2, flight.Laps.Count);
//        }
//    }
//}
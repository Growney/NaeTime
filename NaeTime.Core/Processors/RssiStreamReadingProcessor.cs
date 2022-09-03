using NaeTime.Abstractions.Enumeration;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Processors;

namespace NaeTime.Core.Processors
{
    public class RssiStreamReadingProcessor : IRssiStreamReadingProcessor, IFlightProcessor
    {
        private readonly Track _flightTrack;
        private readonly Flight _streamFlight;
        private readonly RssiStream _stream;

        private readonly List<Lap> _completedLaps = new();
        private readonly List<Lap> _startedLaps = new();
        private readonly List<RssiStreamPass> _completedPasses = new();
        private readonly List<RssiStreamPass> _startedPasses = new();
        private readonly List<Split> _completedSplits = new();
        private readonly List<Split> _startedSplits = new();
        private readonly List<RssiStreamReading> _readings = new();

        public RssiStreamReadingProcessor(Track flightTrack, Flight streamFlight, RssiStream stream)
        {
            _flightTrack = flightTrack;
            //sort the gates to make sure they are in order by position
            _flightTrack.Gates.Sort((x, y) => x.Position.CompareTo(y.Position));
            _stream = stream;
            _streamFlight = streamFlight;
        }

        public IEnumerable<Lap> GetCompletedLaps() => _completedLaps;
        public IEnumerable<RssiStreamPass> GetCompletedPasses() => _completedPasses;
        public IEnumerable<Split> GetCompletedSplits() => _completedSplits;
        public IEnumerable<RssiStreamReading> GetGeneratedReadings() => _readings;
        public IEnumerable<Lap> GetStartedLaps() => _startedLaps;
        public IEnumerable<RssiStreamPass> GetStartedPasses() => _startedPasses;
        public IEnumerable<Split> GetStartedSplits() => _startedSplits;

        public void ProcessReading(RssiStreamReading reading)
        {
            _stream.Readings.Add(reading);
            _readings.Add(reading);

            var readingPass = _stream.Passes.FirstOrDefault(x => reading.Tick > x.Start && x.End == null);
            var boundary = GetBoundary(_stream);
            //We are not in a pass and the reading is above the boundary
            if (reading.Value >= boundary.PeakStartRssi && readingPass == null)
            {
                var pass = new RssiStreamPass()
                {
                    RssiStreamId = _stream.Id,
                    Start = reading.Tick,
                    End = null,
                };
                _stream.Passes.Add(pass);
                HandleStartedPass(pass);
            }
            //We are in a pass and the value has dropped below the boundary
            else if (reading.Value < boundary.PeakEndRssi && readingPass != null)
            {
                readingPass.End = reading.Tick;
                HandleCompletedPass(readingPass);
            }
        }

        private void HandleStartedPass(RssiStreamPass pass)
        {
            _startedPasses.Add(pass);

        }
        private void HandleCompletedPass(RssiStreamPass pass)
        {
            _completedPasses.Add(pass);
            var passGate = _flightTrack.Gates.FirstOrDefault(x => x.NodeId == _stream.NodeId);
            if (passGate != null)
            {
                HandlePassLap(passGate, pass);
                HandlePassSplits(passGate, pass);
            }
        }
        private void HandlePassSplits(TimedGate passGate, RssiStreamPass pass)
        {
            var passGateIndex = _flightTrack.Gates.IndexOf(passGate);
            var nextGateIndex = (passGateIndex + 1) % _flightTrack.Gates.Count;
            var nextGate = _flightTrack.Gates[nextGateIndex];

            var passSplit = _streamFlight.Splits.FirstOrDefault(x => x.EndGate == passGate.Id && x.EndTick == null);
            //Likely the first ever split if this is null
            if (passSplit != null)
            {
                passSplit.EndTick = pass.GetMidPoint();
                HandleCompletedSplit(passSplit);
            }
            passSplit = new Split()
            {
                FlightId = _flightTrack.Id,
                Position = passGateIndex,
                StartGate = passGate.Id,
                StartTick = pass.GetMidPoint(),
                EndGate = nextGate.Id,
            };
            _streamFlight.Splits.Add(passSplit);
            HandleStartedSplit(passSplit);
        }

        private void HandlePassLap(TimedGate passGate, RssiStreamPass pass)
        {
            //We only need to check for laps on gate position 0
            if (passGate.Position == 0)
            {
                var passLap = _streamFlight.Laps.FirstOrDefault(x => x.EndTick == null);
                //When this is null its likely to be the first lap
                if (passLap != null)
                {
                    if (pass.GetMidPoint() - passLap.StartTick > _flightTrack.MinimumLapTime)
                    {
                        passLap.EndTick = pass.GetMidPoint();
                        HandleCompletedLap(passLap);
                    }
                }
                var nextLap = new Lap()
                {
                    FlightId = _streamFlight.Id,
                    StartTick = pass.GetMidPoint(),
                };
                _streamFlight.Laps.Add(nextLap);
                HandleStartedLap(nextLap);
            }
        }

        private void HandleStartedLap(Lap lap)
        {
            _startedLaps.Add(lap);
        }
        private void HandleCompletedLap(Lap lap)
        {
            _completedLaps.Add(lap);
        }
        private void HandleStartedSplit(Split split)
        {
            _startedSplits.Add(split);
        }
        private void HandleCompletedSplit(Split split)
        {
            _completedSplits.Add(split);
        }
        private RssiBoundary GetBoundary(RssiStream stream) =>
            stream.Boundary ??
            stream.ReceiverType switch
            {
                RssiReceiverType.RX5808 => RssiBoundary.RX5808Default,
                _ => new RssiBoundary() { PeakEndRssi = 0, PeakStartRssi = 0 }
            };
    }
}

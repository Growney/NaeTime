using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Enumeration;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Processors;

namespace NaeTime.Core.Processors
{
    public class FlightProcessor : BackgroundService
    {
        private class ProcessedItems<T>
        {
            public List<T> Started { get; } = new();
            public List<T> Completed { get; } = new();
        }
        private class Processed
        {
            public ProcessedItems<Lap> Laps { get; } = new();
            public ProcessedItems<Split> Splits { get; } = new();
            public ProcessedItems<RssiStreamPass> Passes { get; } = new();
            public List<RssiStreamReadingBatch> Batches { get; } = new();
        }
        private readonly List<Lap> _completedLaps = new();
        private readonly List<Lap> _startedLaps = new();
        private readonly List<RssiStreamPass> _completedPasses = new();
        private readonly List<RssiStreamPass> _startedPasses = new();
        private readonly List<Split> _completedSplits = new();
        private readonly List<Split> _startedSplits = new();
        private readonly List<RssiStreamReading> _readings = new();

        private readonly IServiceProvider _serviceProvider;

        public FlightProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));

            while (!stoppingToken.IsCancellationRequested)
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                if (!stoppingToken.IsCancellationRequested)
                {
                    using var serviceProviderScope = _serviceProvider.CreateScope();
                    var unitOfWork = serviceProviderScope.ServiceProvider.GetService<INaeTimeUnitOfWork>();
                    if(unitOfWork != null)
                    {
                        var activeFlights = await unitOfWork.Flights.GetActiveAsync();
                        if(activeFlights.Count > 0)
                        {
                            var processTasks = new List<Task>();
                            foreach(var activeFlight in activeFlights)
                            {
                                processTasks.Add(ProcessActiveFlight(unitOfWork,activeFlight));
                            }
                            await Task.WhenAll(processTasks);
                        }
                        await unitOfWork.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task ProcessActiveFlight(INaeTimeUnitOfWork unitOfWork,Flight flight)
        {
            var batches = (from rssiStream in flight.RssiStreams
                                      from rssiBatch in rssiStream.RssiReadingBatches
                                      select rssiBatch).ToList();
            batches.Sort((x, y) => x.MinTick.CompareTo(y.MinTick));

            var firstUnprocessed = batches.FirstOrDefault(x => x.Processed == null);
            if(firstUnprocessed != null)
            {
                var unprocessedIndex = batches.IndexOf(firstUnprocessed);
                var batchesIdForProcessing = batches.Skip(unprocessedIndex).Select(x => x.Id);
                var batchesForProcessing = await unitOfWork.RssiStreamReadingBatches.GetWithReadingsAsync(batchesIdForProcessing);
                batchesForProcessing.Sort((x, y) => x.MinTick.CompareTo(y.MinTick));
            }

            if(batches.Count > 0)
            {
                foreach(var unprocessedBatch in batches)
                {
                    var batchStream = flight.RssiStreams.FirstOrDefault(x => x.Id == unprocessedBatch.RssiStreamId);
                    if(batchStream != null)
                    {
                        var boundary = GetBoundary(batchStream);
                        var batchPass = batchStream.RssiReadingPasses.FirstOrDefault(x => x.End == null && unprocessedBatch.MinRssiValue > x.Start);
                        //When the batch pass is null we are not in the middle of a pass between two batches and we only need to check the batch if the max rssi is above the threshold
                        if(batchPass == null)
                        {
                            if(unprocessedBatch.MaxRssiValue > boundary.PeakStartRssi)
                            {
                                var batchReadings = await unitOfWork.RssiStreamReadingBatches.GetWithReadingsAsync(unprocessedBatch.Id);
                            }
                        }
                        else
                        {

                        }
                    }
                    unprocessedBatch.Processed = DateTime.UtcNow;
                }
                
            }
        }

        public IEnumerable<Lap> GetCompletedLaps() => _completedLaps;
        public IEnumerable<RssiStreamPass> GetCompletedPasses() => _completedPasses;
        public IEnumerable<Split> GetCompletedSplits() => _completedSplits;
        public IEnumerable<RssiStreamReading> GetGeneratedReadings() => _readings;
        public IEnumerable<Lap> GetStartedLaps() => _startedLaps;
        public IEnumerable<RssiStreamPass> GetStartedPasses() => _startedPasses;
        public IEnumerable<Split> GetStartedSplits() => _startedSplits;

        public void ProcessReading(Flight flight, Track flightTrack, RssiStream stream, RssiStreamReading reading)
        {
            _readings.Add(reading);

            var readingPass = stream.RssiReadingPasses.FirstOrDefault(x => reading.Tick > x.Start && x.End == null);
            var boundary = GetBoundary(stream);
            //We are not in a pass and the reading is above the boundary
            if (reading.Value >= boundary.PeakStartRssi && readingPass == null)
            {
                var pass = new RssiStreamPass()
                {
                    RssiStreamId = stream.Id,
                    Start = reading.Tick,
                    End = null,
                };
                stream.RssiReadingPasses.Add(pass);
                HandleStartedPass(pass);
            }
            //We are in a pass and the value has dropped below the boundary
            else if (reading.Value < boundary.PeakEndRssi && readingPass != null)
            {
                readingPass.End = reading.Tick;
                HandleCompletedPass(flight,flightTrack,stream,readingPass);
            }
        }
        private void HandleStartedPass(RssiStreamPass pass)
        {
            _startedPasses.Add(pass);

        }
        private void HandleCompletedPass(Flight flight,Track flightTrack,RssiStream stream,RssiStreamPass pass)
        {
            _completedPasses.Add(pass);
            var passGate = flightTrack.Gates.FirstOrDefault(x => x.NodeId == stream.NodeId);
            if (passGate != null)
            {
                HandlePassLap(flight,flightTrack,passGate, pass);
                HandlePassSplits(flight, flightTrack, passGate, pass);
            }
        }
        private void HandlePassSplits(Flight flight,Track flightTrack, TimedGate passGate, RssiStreamPass pass)
        {
            var passGateIndex = flightTrack.Gates.IndexOf(passGate);
            var nextGateIndex = (passGateIndex + 1) % flightTrack.Gates.Count;
            var nextGate = flightTrack.Gates[nextGateIndex];

            var passSplit = flight.Splits.FirstOrDefault(x => x.EndGate == passGate.Id && x.EndTick == null);
            //Likely the first ever split if this is null
            if (passSplit != null)
            {
                passSplit.EndTick = pass.GetMidPoint();
                HandleCompletedSplit(passSplit);
            }
            passSplit = new Split()
            {
                FlightId = flightTrack.Id,
                Position = passGateIndex,
                StartGate = passGate.Id,
                StartTick = pass.GetMidPoint(),
                EndGate = nextGate.Id,
            };
            flight.Splits.Add(passSplit);
            HandleStartedSplit(passSplit);
        }
        private void HandlePassLap(Flight flight, Track flightTrack,TimedGate passGate, RssiStreamPass pass)
        {
            //We only need to check for laps on gate position 0
            if (passGate.Position == 0)
            {
                var passLap = flight.Laps.FirstOrDefault(x => x.EndTick == null);
                //When this is null its likely to be the first lap
                if (passLap != null)
                {
                    if (pass.GetMidPoint() - passLap.StartTick > flightTrack.MinimumLapTime)
                    {
                        passLap.EndTick = pass.GetMidPoint();
                        HandleCompletedLap(passLap);
                    }
                }
                var nextLap = new Lap()
                {
                    StartTick = pass.GetMidPoint(),
                };
                flight.Laps.Add(nextLap);
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

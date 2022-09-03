using NaeTime.Abstractions.Handlers;
using NaeTime.Abstractions.Processors;

namespace NaeTime.Core.Processors
{
    public class HandlerProcessor : IHandlerProcessor
    {
        private readonly IEnumerable<IFlightLapHandler> _lapHandlers;
        private readonly IEnumerable<IFlightSplitHandler> _splitHandlers;
        private readonly IEnumerable<IRssiStreamReadingHandler> _rssiReadingHandlers;
        private readonly IEnumerable<IRssiStreamPassHander> _rssiStreamPassHandlers;

        public HandlerProcessor(IEnumerable<IFlightLapHandler> lapHandlers, IEnumerable<IFlightSplitHandler> splitHandlers, IEnumerable<IRssiStreamReadingHandler> rssiReadingHandlers, IEnumerable<IRssiStreamPassHander> rssiStreamPassHandlers)
        {
            _lapHandlers = lapHandlers ?? throw new ArgumentNullException(nameof(lapHandlers));
            _splitHandlers = splitHandlers ?? throw new ArgumentNullException(nameof(splitHandlers));
            _rssiReadingHandlers = rssiReadingHandlers ?? throw new ArgumentNullException(nameof(rssiReadingHandlers));
            _rssiStreamPassHandlers = rssiStreamPassHandlers ?? throw new ArgumentNullException(nameof(rssiStreamPassHandlers));
        }

        public void HandleProcessedFlights(IFlightProcessor processor)
        {
            if (_lapHandlers.Any())
            {
                HandleStartedLaps(processor);
                HandleCompletedLaps(processor);
            }
            if (_splitHandlers.Any())
            {
                HandleStartedSplits(processor);
                HandleCompletedSplits(processor);
            }
        }

        private void HandleStartedSplits(IFlightProcessor processor)
        {
            foreach (var startedSplit in processor.GetStartedSplits())
            {
                foreach (var splitHandler in _splitHandlers)
                {
                    splitHandler.HandleStartedSplit(startedSplit);
                }
            }
        }
        private void HandleCompletedSplits(IFlightProcessor processor)
        {
            foreach (var completedSplit in processor.GetCompletedSplits())
            {
                foreach (var splitHandler in _splitHandlers)
                {
                    splitHandler.HandleCompletedSplit(completedSplit);
                }
            }
        }
        private void HandleCompletedLaps(IFlightProcessor processor)
        {
            foreach (var completedLap in processor.GetCompletedLaps())
            {
                foreach (var lapHandler in _lapHandlers)
                {
                    lapHandler.HandleCompletedLap(completedLap);
                }
            }
        }
        private void HandleStartedLaps(IFlightProcessor processor)
        {
            foreach (var startedLap in processor.GetStartedLaps())
            {
                foreach (var lapHandler in _lapHandlers)
                {
                    lapHandler.HandleStartedLap(startedLap);
                }
            }
        }

        public void HandleProcessedStreams(IRssiStreamReadingProcessor processor)
        {
            if (_rssiReadingHandlers.Any())
            {
                HandleStreamReadings(processor);
            }
            if (_rssiStreamPassHandlers.Any())
            {
                HandleStartedPasses(processor);
                HandleCompletedPasses(processor);
            }
        }

        private void HandleStartedPasses(IRssiStreamReadingProcessor processor)
        {
            foreach (var startedPass in processor.GetStartedPasses())
            {
                foreach (var passHandler in _rssiStreamPassHandlers)
                {
                    passHandler.HandleStartedPass(startedPass);
                }
            }
        }

        private void HandleCompletedPasses(IRssiStreamReadingProcessor processor)
        {
            foreach (var completedPass in processor.GetCompletedPasses())
            {
                foreach (var passHandler in _rssiStreamPassHandlers)
                {
                    passHandler.HandleCompletedPass(completedPass);
                }
            }
        }

        private void HandleStreamReadings(IRssiStreamReadingProcessor processor)
        {
            foreach (var reading in processor.GetGeneratedReadings())
            {
                foreach (var readingHandler in _rssiReadingHandlers)
                {
                    readingHandler.HandleReading(reading);
                }
            }
        }
    }
}

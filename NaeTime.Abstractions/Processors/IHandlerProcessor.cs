namespace NaeTime.Abstractions.Processors
{
    public interface IHandlerProcessor
    {
        void HandleProcessedStreams(IRssiStreamReadingProcessor processor);
        void HandleProcessedFlights(IFlightProcessor processor);
    }
}

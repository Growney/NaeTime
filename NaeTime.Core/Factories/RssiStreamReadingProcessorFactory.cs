using NaeTime.Abstractions;
using NaeTime.Abstractions.Factories;
using NaeTime.Abstractions.Processors;
using NaeTime.Core.Processors;

namespace NaeTime.Core.Factories
{
    public class RssiStreamReadingProcessorFactory : IRssiStreamReadingProcessorFactory
    {
        private readonly INaeTimeUnitOfWork _unitOfWork;

        public RssiStreamReadingProcessorFactory(INaeTimeUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IRssiStreamReadingProcessor> CreateProcessorAsync(Guid streamId)
        {
            var flight = await _unitOfWork.Flights.GetForStreamAsync(streamId);
            if (flight == null)
            {
                throw new InvalidOperationException("Stream_flight_not_found");
            }
            var stream = flight.RssiStreams.FirstOrDefault(x => x.Id == streamId);

            if (stream == null)
            {
                throw new InvalidOperationException("Stream_not_found");
            }

            var track = await _unitOfWork.Tracks.GetAsync(flight.TrackId);

            if (track == null)
            {
                throw new InvalidOperationException("Track_not_found");
            }
            return new RssiStreamReadingProcessor(track, flight, stream);
        }

        public Task SaveChangedAsync() => _unitOfWork.SaveChangesAsync();
    }
}

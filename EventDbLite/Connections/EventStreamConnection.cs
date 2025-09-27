using EventDbLite.Abstractions;
using EventDbLite.DbModels;
using EventDbLite.Exceptions;
using EventDbLite.Streams;
using Microsoft.EntityFrameworkCore;

namespace EventDbLite.Connections;

internal class EventStreamConnection : IEventStreamConnection
{
    private readonly EventDbLiteContext _context;

    public EventStreamConnection(EventDbLiteContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<StreamEvent>> AppendToStreamAsync(string streamName, IEnumerable<EventData> data, StreamPosition expectedState)
    {
        if (expectedState == StreamPosition.NoStream)
        {
            if (await _context.PersistedEvents.AnyAsync(s => s.StreamName == streamName))
            {
                throw new InvalidOperationException($"Stream '{streamName}' already exists.");
            }
        }

        if (expectedState == StreamPosition.StreamExists)
        {
            if (!await _context.PersistedEvents.AnyAsync(s => s.StreamName == streamName))
            {
                throw new InvalidOperationException($"Stream '{streamName}' does not exist.");
            }
        }

        long currentStreamVersion = await _context.PersistedEvents.Where(x => x.StreamName == streamName).OrderByDescending(x => x.StreamOrdinal).Select(x => x.StreamOrdinal).FirstOrDefaultAsync();
        List<PersistedEvent> createdPersistedEvents = new();

        foreach (var eventData in data)
        {
            var newEvent = new DbModels.PersistedEvent()
            {
                Id = Guid.NewGuid(),
                StreamName = streamName,
                StreamOrdinal = ++currentStreamVersion,
                Metadata = eventData.Metadata,
                Payload = eventData.Payload,
            };
            createdPersistedEvents.Add(newEvent);
            _context.PersistedEvents.Add(newEvent);
        }

        if (!expectedState.IsValidUpdateVersion(currentStreamVersion))
        {
            throw new ConcurrencyException(expectedState.Version, currentStreamVersion);
        }

        try
        {
            await _context.SaveChangesAsync();

            return createdPersistedEvents.Select(x => new StreamEvent(x.Id, x.StreamName, x.StreamOrdinal, x.GlobalOrdinal, new EventData(x.Payload, x.Metadata)));
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(expectedState.Version, currentStreamVersion);
        }
    }
    public async Task<StreamEvent> AppendToStreamAsync(string streamName, EventData data, StreamPosition expectedState) => (await AppendToStreamAsync(streamName, Enumerable.Repeat(data, 1), expectedState)).First();
    public void Dispose() => _context.Dispose();
    public IAsyncEnumerable<StreamEvent> ReadAllStreamEvents(StreamDirection direction, StreamPosition position)
    {

        var query = _context.PersistedEvents.AsQueryable();
        query = direction == StreamDirection.Forward ? query.Where(x => x.GlobalOrdinal >= position.Version) : query.Where(x => x.GlobalOrdinal <= position.Version);
        query = direction == StreamDirection.Forward ? query.OrderBy(x => x.GlobalOrdinal) : query.OrderByDescending(x => x.GlobalOrdinal);
        return query
            .Select(x => new StreamEvent(x.Id, x.StreamName, x.StreamOrdinal, x.GlobalOrdinal, new EventData(x.Payload, x.Metadata)))
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<StreamEvent> ReadStreamEvents(string streamName, StreamDirection direction, StreamPosition position)
    {
        var query = _context.PersistedEvents
                .Where(x => x.StreamName == streamName);

        query = direction == StreamDirection.Forward ? query.Where(x => x.StreamOrdinal >= position.Version) : query.Where(x => x.StreamOrdinal <= position.Version);
        query = direction == StreamDirection.Forward ? query.OrderBy(x => x.StreamOrdinal) : query.OrderByDescending(x => x.StreamOrdinal);

        return query
            .Select(x => new StreamEvent(x.Id, x.StreamName, x.StreamOrdinal, x.GlobalOrdinal, new EventData(x.Payload, x.Metadata)))
            .AsAsyncEnumerable();
    }
}

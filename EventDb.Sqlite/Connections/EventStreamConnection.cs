using EventDb.Sqlite.Abstractions;
using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using EventDbLite.Streams;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventDb.Sqlite.Connections;

internal class EventStreamConnection(ISqliteConnectionFactory connectionFactory) : IEventStreamConnection
{
    private readonly ISqliteConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public async Task<IEnumerable<StreamEvent>> AppendToStreamAsync(string streamName, IEnumerable<EventData> data, StreamPosition expectedState)
    {
        await _semaphore.WaitAsync();
        try
        {
            SqliteConnection sqliteConnection = _connectionFactory.CreateConnection(SqliteOpenMode.ReadWrite);

            sqliteConnection.Open();

            SqliteTransaction transaction = sqliteConnection.BeginTransaction(deferred: true);

            if (expectedState == StreamPosition.NoStream || expectedState == StreamPosition.StreamExists)
            {
                using (SqliteCommand checkNoStreamCommand = sqliteConnection.CreateCommand())
                {
                    checkNoStreamCommand.CommandText =
                        @"SELECT COUNT(1)
                        FROM PersistedEvents
                        WHERE StreamName = $streamName;";
                    checkNoStreamCommand.Parameters.AddWithValue("$streamName", streamName);

                    long existingCount = (long)checkNoStreamCommand.ExecuteScalar()!;

                    if (expectedState == StreamPosition.NoStream && existingCount > 0)
                    {
                        throw new ConcurrencyException(StreamPosition.NoStream, StreamPosition.StreamExists);
                    }
                    else if (expectedState == StreamPosition.StreamExists && existingCount == 0)
                    {
                        throw new ConcurrencyException(StreamPosition.StreamExists, StreamPosition.NoStream);
                    }
                }
            }
            long currentStreamVersion = 0;
            using (SqliteCommand currentStreamVersionCommand = sqliteConnection.CreateCommand())
            {
                currentStreamVersionCommand.CommandText =
                    @"SELECT IFNULL(MAX(StreamOrdinal), 0)
                    FROM PersistedEvents
                    WHERE StreamName = $streamName;";
                currentStreamVersionCommand.Parameters.AddWithValue("$streamName", streamName);
                currentStreamVersion = (long)currentStreamVersionCommand.ExecuteScalar()!;
                if (!expectedState.IsValidUpdateVersion(currentStreamVersion + data.Count()))
                {
                    throw new ConcurrencyException(expectedState.Version, currentStreamVersion);
                }
            }

            using (SqliteCommand writeCommand = sqliteConnection.CreateCommand())
            {
                writeCommand.CommandText =
                    @"INSERT INTO PersistedEvents (Id, StreamName, StreamOrdinal, Payload, Metadata, Identifier)
                    VALUES ($id, $streamName, $streamOrdinal, $payload, $metadata, $identifier);
                    SELECT last_insert_rowid();";
                SqliteParameter idParam = writeCommand.Parameters.Add("$id", SqliteType.Text);
                SqliteParameter streamNameParam = writeCommand.Parameters.Add("$streamName", SqliteType.Text);
                SqliteParameter streamOrdinalParam = writeCommand.Parameters.Add("$streamOrdinal", SqliteType.Integer);
                SqliteParameter payloadParam = writeCommand.Parameters.Add("$payload", SqliteType.Blob);
                SqliteParameter metadataParam = writeCommand.Parameters.Add("$metadata", SqliteType.Blob);
                SqliteParameter identifierParam = writeCommand.Parameters.Add("$identifier", SqliteType.Text);
                List<StreamEvent> createdEvents = new();
                foreach (var eventData in data)
                {
                    idParam.Value = Guid.NewGuid().ToString();
                    streamNameParam.Value = streamName;
                    streamOrdinalParam.Value = ++currentStreamVersion;
                    payloadParam.Value = eventData.Payload;
                    metadataParam.Value = eventData.Metadata;
                    identifierParam.Value = eventData.Identifier;
                    long globalId = 0;
                    using (var reader = writeCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            globalId = reader.GetInt64(0);
                        }
                    }
                    createdEvents.Add(new StreamEvent(
                        Guid.Parse(idParam.Value.ToString()!),
                        streamName,
                        (long)streamOrdinalParam.Value,
                        globalId,
                        new EventData((byte[])payloadParam.Value, (byte[])metadataParam.Value, identifierParam.Value.ToString()!)
                    ));
                }
                transaction.Commit();
                return createdEvents;
            }
        }
        finally
        {
            _semaphore.Release();
        }
        
    }
    public async Task<StreamEvent> AppendToStreamAsync(string streamName, EventData data, StreamPosition expectedState) => (await AppendToStreamAsync(streamName, Enumerable.Repeat(data, 1), expectedState)).First();
    public async IAsyncEnumerable<StreamEvent> ReadAllStreamEvents(StreamDirection direction, StreamPosition position)
    {
        //This feels wrong, using it to allow the use of IasyncEnumerable on a clearly synchronous method
        //Lets see how it get on
        await Task.CompletedTask;

        SqliteConnection sqliteConnection = _connectionFactory.CreateConnection(SqliteOpenMode.ReadOnly);

        sqliteConnection.Open();

        SqliteCommand command = sqliteConnection.CreateCommand();

        try
        {
            command.CommandText =
            @"SELECT Id, StreamName, StreamOrdinal, GlobalOrdinal, Payload, Metadata, Identifier
              FROM PersistedEvents
              WHERE (( $direction = 0 AND GlobalOrdinal > $position ) OR ( $direction = 1 AND GlobalOrdinal < $position ))
              ORDER BY GlobalOrdinal " + (direction == StreamDirection.Forward ? "ASC" : "DESC") + ";";

            command.Parameters.AddWithValue("$direction", direction == StreamDirection.Forward ? 0 : 1);
            command.Parameters.AddWithValue("$position", position.Version);


            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                yield return new StreamEvent(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetInt64(2),
                    reader.GetInt64(3),
                    new EventData(
                        (byte[])reader["Payload"],
                        (byte[])reader["Metadata"],
                        reader.GetString(6)
                    )
                );
            }
        }
        finally
        {
            command.Dispose();
            sqliteConnection.Close();
            sqliteConnection.Dispose();
        }

    }

    public async IAsyncEnumerable<StreamEvent> ReadStreamEvents(string streamName, StreamDirection direction, StreamPosition position)
    {
        //This feels wrong, using it to allow the use of IasyncEnumerable on a clearly synchronous method
        //Lets see how it get on
        await Task.CompletedTask;

        SqliteConnection sqliteConnection = _connectionFactory.CreateConnection(SqliteOpenMode.ReadOnly);

        sqliteConnection.Open();

        SqliteCommand command = sqliteConnection.CreateCommand();

        try
        {
            command.CommandText =
            @"SELECT Id, StreamName, StreamOrdinal, GlobalOrdinal, Payload, Metadata, Identifier
              FROM PersistedEvents
              WHERE StreamName = $streamName
              AND (( $direction = 0 AND StreamOrdinal > $position ) OR ( $direction = 1 AND StreamOrdinal < $position ))
              ORDER BY StreamOrdinal " + (direction == StreamDirection.Forward ? "ASC" : "DESC") + ";";

            command.Parameters.AddWithValue("$streamName", streamName);
            command.Parameters.AddWithValue("$direction", direction == StreamDirection.Forward ? 0 : 1);
            command.Parameters.AddWithValue("$position", position.Version);

            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                yield return new StreamEvent(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetInt64(2),
                    reader.GetInt64(3),
                    new EventData(
                        (byte[])reader["Payload"],
                        (byte[])reader["Metadata"],
                        reader.GetString(6)
                    )
                );
            }
        }
        finally
        {
            command.Dispose();
            sqliteConnection.Close();
            sqliteConnection.Dispose();
        }
    }
}

namespace NaeTime.OpenPractice.Leaderboards;
public class Leaderboard<TRecord> where TRecord : IComparable<TRecord>
{
    private readonly Dictionary<Guid, TRecord> _records = new();

    public void SetFastest(Guid pilotId, TRecord record)
    {
        if (_records.TryGetValue(pilotId, out TRecord? existingRecord))
        {
            if (record.CompareTo(existingRecord) < 0)
            {
                _records[pilotId] = record;
            }
        }
        else
        {
            _records[pilotId] = record;
        }
    }

    public IDictionary<Guid, LeaderboardPosition<TRecord>> GetPositions()
    {
        List<KeyValuePair<Guid, TRecord>> records = _records.ToList();

        records.Sort((x, y) => x.Value.CompareTo(y.Value));

        Dictionary<Guid, LeaderboardPosition<TRecord>> positions = new();

        for (int i = 0; i < records.Count; i++)
        {
            KeyValuePair<Guid, TRecord> record = records[i];
            LeaderboardPosition<TRecord> position = new(record.Key, i, record.Value);
            positions[record.Key] = position;
        }
        return positions;
    }
}

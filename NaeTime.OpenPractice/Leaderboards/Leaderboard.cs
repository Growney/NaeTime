namespace NaeTime.OpenPractice.Leaderboards;
public class Leaderboard<TRecord> where TRecord : IComparable<TRecord>
{
    private readonly Dictionary<Guid, TRecord> _records = new();

    public void SetFastest(Guid pilotId, TRecord record)
    {
        if (_records.TryGetValue(pilotId, out var existingRecord))
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
        var records = _records.ToList();

        records.Sort((x, y) => x.Value.CompareTo(y.Value));

        var positions = new Dictionary<Guid, LeaderboardPosition<TRecord>>();

        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var position = new LeaderboardPosition<TRecord>(record.Key, i, record.Value);
            positions[record.Key] = position;
        }
        return positions;
    }
}

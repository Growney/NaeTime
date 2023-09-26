namespace NaeTime.Node.WebAPI.Map;

public class RssiReadingMap : IUnidirectionalMapper<RssiReading, RssiReadingDto>
{
    public RssiReadingDto Map(RssiReading source, RssiReadingDto? destination)
    {
        return new RssiReadingDto()
        {
            Tick = source.Tick,
            Value = source.Value,
        };
    }
}

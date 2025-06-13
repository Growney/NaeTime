using EventDbLite.Projections;

namespace EventDbLite.Abstractions;

public interface IProjectionProvider
{
    Task<T> Load<T>(string? streamName = null) where T : Projection;
}

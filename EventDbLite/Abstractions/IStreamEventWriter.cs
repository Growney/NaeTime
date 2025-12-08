namespace EventDbLite.Abstractions;
public interface IStreamEventWriter
{
    Task AppendToStream(string streamName, IEnumerable<object> eventObjs);
}

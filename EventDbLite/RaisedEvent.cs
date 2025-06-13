
using System.Reflection;

namespace EventDbLite;

public class RaisedEvent
{
    public RaisedEvent(string identifier, object data)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public string Identifier { get; }
    public object Data { get; }

    public static string GetIdentifier(Type eventType)
    {
        EventAttribute? commandAttribute = eventType.GetCustomAttribute<EventAttribute>();
        if (commandAttribute != null)
        {
            return commandAttribute.Identifier;
        }
        else
        {
            return eventType.Name;
        }
    }
}

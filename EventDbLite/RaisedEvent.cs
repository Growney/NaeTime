
using System.Reflection;

namespace EventDbLite;

public class RaisedEvent(string identifier, object data)
{
    public string Identifier { get; } = identifier ?? throw new ArgumentNullException(nameof(identifier));
    public object Data { get; } = data ?? throw new ArgumentNullException(nameof(data));

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

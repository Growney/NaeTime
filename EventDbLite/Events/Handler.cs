namespace EventDbLite.Events;

public class Handler
{
    public Handler(Action<object> action, Type targetType)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }

    public Action<object> Action { get; }
    public Type TargetType { get; }
}

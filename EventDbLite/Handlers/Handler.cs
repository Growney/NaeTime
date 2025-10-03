namespace EventDbLite.Handlers;

public class Handler
{
    public Handler(Action<object, object> action, Type targetType)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }

    public Action<object, object> Action { get; }
    public Type TargetType { get; }
}

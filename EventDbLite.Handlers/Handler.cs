namespace EventDbLite.Handlers;

public class Handler(Action<object, object> action, Type targetType)
{
    public Action<object, object> Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
    public Type TargetType { get; } = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

namespace EventDbLite.Handlers;

public class AsyncHandler(Func<object, object, Task> action, Type targetType)
{
    public Func<object, object, Task> Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
    public Type TargetType { get; } = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

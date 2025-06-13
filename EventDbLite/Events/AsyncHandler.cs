namespace EventDbLite.Events;

public class AsyncHandler
{
    public AsyncHandler(Func<object, Task> action, Type targetType)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }

    public Func<object, Task> Action { get; }
    public Type TargetType { get; }
}

namespace EventDbLite.Handlers;

public class CommandHandler(Type targetType, Func<object, Task<bool>> action)
{
    public Type TargetType { get; } = targetType ?? throw new ArgumentNullException(nameof(targetType));
    public Func<object, Task<bool>> Action { get; } = action ?? throw new ArgumentNullException(nameof(action));
}

namespace EventDbLite.Handlers;

public class CommandHandler
{
    public CommandHandler(Type targetType, Func<object, Task<bool>> action)
    {
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Type TargetType { get; }
    public Func<object, Task<bool>> Action { get; }
}

namespace EventDbLite.Reactions;
internal class ConstantReaction(Func<IServiceProvider, object, Task> handler, Type targetType)
{
    public Func<IServiceProvider, object, Task> Handler { get; } = handler ?? throw new ArgumentNullException(nameof(handler));
    public Type TargetType { get; } = targetType ?? throw new ArgumentNullException(nameof(targetType));
}

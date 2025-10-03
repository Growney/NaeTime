namespace EventDbLite.Reactions;
internal class ConstantReaction
{
    public Func<IServiceProvider, object, Task> Handler { get; }
    public Type TargetType { get; }

    public ConstantReaction(Func<IServiceProvider, object, Task> handler, Type targetType)
    {
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }
}

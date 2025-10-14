namespace EventDbLite.Reactions;
internal class ConstantReactionSource(IEnumerable<ConstantReaction> reactions)
{
    public IEnumerable<ConstantReaction> Reactions { get; } = reactions ?? throw new ArgumentNullException(nameof(reactions));
}

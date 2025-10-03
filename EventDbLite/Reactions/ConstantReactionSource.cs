namespace EventDbLite.Reactions;
internal class ConstantReactionSource
{
    public IEnumerable<ConstantReaction> Reactions { get; }

    public ConstantReactionSource(IEnumerable<ConstantReaction> reactions)
    {
        Reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
    }
}

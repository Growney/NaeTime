namespace EventDbLite.Reactions;
internal class ConstantReactionSource
{
    public IEnumerable<ConstantReaction> Reactions { get; }
    public string? ReactionKey { get; }

    public ConstantReactionSource(IEnumerable<ConstantReaction> reactions, string? reactionKey = null)
    {
        Reactions = reactions ?? throw new ArgumentNullException(nameof(reactions));
        ReactionKey = reactionKey;
    }
}

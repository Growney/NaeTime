namespace ELRS.Backpack;

public class BackpackStatus
{
    public BackpackStatus(bool isWifiEnabled, bool isBindInProgress, bool isBound, byte[]? boundUId)
    {
        IsWifiEnabled = isWifiEnabled;
        IsBindInProgress = isBindInProgress;
        IsBound = isBound;
        BoundUId = boundUId;
    }

    public bool IsWifiEnabled { get; }
    public bool IsBindInProgress { get; }
    public bool IsBound { get; }
    public byte[]? BoundUId { get; }
}

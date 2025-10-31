namespace EventDbLite.Abstractions;
public interface ILiveProjectionRepository
{
    public void RegisterManager(Type projectionServiceType, ILiveProjectionManager manager);
    public ILiveProjectionManager? GetManager(Type projectionServiceType);
}

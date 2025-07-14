namespace EventDbLite.Abstractions;

public interface ICommandRouter
{
    Task<bool> Handle(object command);
}

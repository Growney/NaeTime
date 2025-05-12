namespace ELRS.Backpack;

public interface IBackpackConnectionFactory
{
    IBackpackConnection Create(string port);
}
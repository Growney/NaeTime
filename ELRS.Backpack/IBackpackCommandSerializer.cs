namespace ELRS.Backpack;

public interface IBackpackCommandSerializer
{
    byte[] Serialize(BackpackCommand command);
}
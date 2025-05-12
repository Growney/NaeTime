namespace ELRS.Backpack;

internal interface IBackpackCommandSerializer
{
    byte[] Serialize(BackpackCommand command);
}
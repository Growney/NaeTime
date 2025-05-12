namespace ELRS.Backpack;

public enum CommandType : byte
{
    Request = (byte)'<',
    Response = (byte)'>',
    Error = (byte)'!',
}

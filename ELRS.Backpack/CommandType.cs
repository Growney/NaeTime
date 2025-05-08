namespace ELRS.Backpack;

public enum CommandType
{
    Request = (byte)'<',
    Response = (byte)'>',
    Error = (byte)'!',
}

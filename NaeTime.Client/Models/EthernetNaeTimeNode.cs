namespace NaeTime.Client.Models;
public class EthernetNaeTimeNode : NaeTimeNode
{
    public string? IPAddress { get; set; }
    public ushort Port { get; set; }
}

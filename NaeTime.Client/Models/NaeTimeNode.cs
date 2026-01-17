namespace NaeTime.Client.Models;
public class NaeTimeNode
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public List<NaeTimeNodeLane> Lanes { get; set; } = [];
}

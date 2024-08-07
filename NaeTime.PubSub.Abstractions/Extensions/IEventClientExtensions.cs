namespace NaeTime.PubSub.Abstractions;
public static class IEventClientExtensions
{
    /// <summary>
    /// Fire and forget publish
    /// </summary>
    /// <param name="client"></param>
    /// <param name="obj"></param>
    public static void Publish(this IEventClient client, object obj)
    {
        try
        {
            _ = client.PublishAsync(obj);
        }
        catch
        {

        }
    }
}

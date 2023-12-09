namespace NaeTime.Client.Razor.Lib.Abstractions;
public static class IApiConfigurationExtensions
{
    public static async Task<T?> GetPropertyObjectValue<T>(this IApiConfiguration configuration, int id)
        where T : class
    {
        var objectValue = await configuration.GetPropertyValueAsync(id);

        if (objectValue == null)
        {
            return null;
        }

        return (T)objectValue;

    }
    public static async Task<T?> GetPropertyValue<T>(this IApiConfiguration configuration, int id)
        where T : struct
    {
        var objectValue = await configuration.GetPropertyValueAsync(id);

        if (objectValue == null)
        {
            return null;
        }

        return (T)objectValue;
    }
}

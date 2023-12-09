namespace NaeTime.Client.Razor.Lib.Models;
public class ApiConfigurationProperty
{
    public ApiConfigurationProperty(int id, string displayName, Type valueType)
    {
        Id = id;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
    }

    public int Id { get; }
    public string DisplayName { get; }
    public Type ValueType { get; }
}

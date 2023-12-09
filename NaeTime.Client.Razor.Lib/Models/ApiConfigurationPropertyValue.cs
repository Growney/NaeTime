namespace NaeTime.Client.Razor.Lib.Models;
public class ApiConfigurationPropertyValue
{
    public ApiConfigurationPropertyValue(int id, object? value)
    {
        Id = id;
        Value = value;
    }

    public int Id { get; }
    public object? Value { get; set; }
}

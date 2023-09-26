using System.Text;

namespace EventStore.Helpers;

public static class StreamHelper
{
    private const char c_separator = '-';
    public static string GetStreamName(params object[] nameComponents)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < nameComponents.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(c_separator);
            }
            builder.Append(nameComponents[i].ToString());
        }
        return builder.ToString();
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EventDbLite.Abstractions;

public static class IStreamEventWriterExtensions
{
    public static Task AppendToStream(this IStreamEventWriter writer,string streamName, object eventObj)
    {
        return writer.AppendToStream(streamName, Enumerable.Repeat(eventObj, 1));
    }
}

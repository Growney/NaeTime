using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Serialization.Abstractions
{
    public interface ISerializer
    {
        byte[] Serialize(object data);
        string SerializeToString(object data);
    }
}

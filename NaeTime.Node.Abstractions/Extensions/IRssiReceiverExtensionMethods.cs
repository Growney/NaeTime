using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions.Extensions
{
    public static class IRssiReceiverExtensionMethods
    {
        public static bool IsStreamEnabled(this IRssiReceiver receiver) => receiver.CurrentStream != null;
    }
}

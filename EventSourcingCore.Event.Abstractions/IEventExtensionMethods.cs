using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventSourcingCore.Event.Abstractions
{
    public static class IEventExtensionMethods
    {
        public static string GetIdentifier(this IEvent eventObj)
        {
            return eventObj.GetType().GetEventIdentifier();
        }
        public static string GetEventIdentifier(this Type eventType)
        {
            EventAttribute commandAttribute = eventType?.GetCustomAttribute<EventAttribute>();
            if (commandAttribute != null)
            {
                return commandAttribute.Identifier;
            }
            else
            {
                return eventType.Name;
            }
        }
    }
}

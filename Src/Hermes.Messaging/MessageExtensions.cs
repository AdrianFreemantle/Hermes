using System;
using System.Linq;

using Hermes.Messaging.Configuration;

namespace Hermes.Messaging
{
    public static class MessageExtensions
    {
        public static Type[] GetContracts(this object message)
        {
            var messageType = message.GetType();

            return (Settings.IsCommandType(messageType) || Settings.IsMessageType(messageType))
                ? new[] { messageType }
                : message.GetType().GetInterfaces().ToArray();
        }
    }
}
using System;
using System.Linq;
using System.ServiceModel;
using Hermes.Equality;
using Hermes.Messaging.Configuration;

namespace Hermes.Messaging
{
    public static class MessageExtensions
    {
        private static readonly TypeEqualityComparer EqualityComparer = new TypeEqualityComparer();

        public static Type[] GetContracts(this object message)
        {
            return GetContracts(message.GetType());
        }

        public static Type[] GetContracts(this Type messageType)
        {
            if (Settings.IsCommandType(messageType) || Settings.IsMessageType(messageType))
            {
                return new[] { messageType };
            }

            if (Settings.IsEventType(messageType))
            {
                return messageType
                    .GetInterfaces()
                    .Union(new[] { messageType })
                    .Distinct(EqualityComparer)
                    .ToArray();
            }

            throw new InvalidMessageContractException(String.Format("The type {0} contains no known message contract types.", messageType.FullName));
        }
    }
}
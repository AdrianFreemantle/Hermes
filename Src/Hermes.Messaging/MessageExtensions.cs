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
            var messageType = message.GetType();

            if (Settings.IsCommandType(messageType) || Settings.IsMessageType(messageType))
            {
                return new[] {messageType};
            }
            
            if(Settings.IsEventType(messageType))
            {
                return message.GetType()
                    .GetInterfaces()
                    .Union(new[] { messageType })
                    .Distinct(EqualityComparer)
                    .ToArray();
            }

            throw new InvalidMessageContractException(String.Format("The type {0} contains no known message contract types.", messageType.FullName));
        }
    }
}
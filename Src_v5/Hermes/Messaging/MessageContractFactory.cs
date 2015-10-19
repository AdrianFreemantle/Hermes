using System;
using System.Linq;
using System.ServiceModel;
using Hermes.Equality;

namespace Hermes.Messaging
{
    public static class MessageContractFactory
    {
        private static readonly TypeEqualityComparer EqualityComparer = new TypeEqualityComparer();

        public static Func<Type, bool> IsMessageType { get; set; }
        public static Func<Type, bool> IsCommandType { get; set; }
        public static Func<Type, bool> IsEventType { get; set; }

        static MessageContractFactory()
        {
            IsMessageType = type => false;
            IsCommandType = type => false;
            IsEventType = type => false;
        }

        public static Type[] GetContracts(object message)
        {
            var messageType = message.GetType();

            if (IsCommandType(messageType) || IsMessageType(messageType))
            {
                return new[] { messageType };
            }

            if (IsEventType(messageType))
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

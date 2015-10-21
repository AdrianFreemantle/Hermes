using System;

namespace Hermes.Messaging
{
    public static class MessageTypeDefinition
    {
        public static Func<Type, bool> IsCommand { get; set; }
        public static Func<Type, bool> IsEvent { get; set; }
        public static Func<Type, bool> IsControl { get; set; }

        static MessageTypeDefinition()
        {
            IsCommand = DefaultIsCommand;
            IsEvent = DefaultIsEvent;
            IsControl = DefaultIsControlMessage;
        }

        private static bool DefaultIsCommand(Type type)
        {
            return MessageIsAssignableFrom<IDomainCommand>(type);
        }

        private static bool DefaultIsEvent(Type type)
        {
            return MessageIsAssignableFrom<IDomainEvent>(type);
        }

        private static bool DefaultIsControlMessage(Type type)
        {
            return MessageIsAssignableFrom<IControlMessage>(type);
        }

        private static bool MessageIsAssignableFrom<TMessageType>(Type type) 
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(TMessageType).IsAssignableFrom(type);
        }
    }
}
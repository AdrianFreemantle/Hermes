namespace Hermes.Messaging
{
    public static class MessageContextExtensions
    {
        public static bool IsCommandMessage(this MessageContext context)
        {
            return MessageTypeDefinition.IsCommand(context.Message.GetType());
        }

        public static bool IsEventMessage(this MessageContext context)
        {
            return MessageTypeDefinition.IsCommand(context.Message.GetType());
        }

        public static bool IsControlMessage(this MessageContext context)
        {
            return MessageTypeDefinition.IsControl(context.Message.GetType());
        }
    }
}
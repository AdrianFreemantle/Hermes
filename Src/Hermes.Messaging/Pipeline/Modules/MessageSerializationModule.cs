using System;
using System.Linq;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline
{
    public class MessageSerializationModule : IModule<OutgoingMessageContext>
    {
        private readonly ISerializeMessages messageSerializer;

        public MessageSerializationModule(ISerializeMessages messageSerializer)
        {
            this.messageSerializer = messageSerializer;
        }

        public void Invoke(OutgoingMessageContext input, Action next)
        {
            input.MessageSerializationFunction(SerializeMessages);
            next();
        }

        public byte[] SerializeMessages(object[] messages)
        {
            if (messages.Any())
            {
                return messageSerializer.Serialize(messages);
            }
             
            return new byte[0];
        }        
    }
}
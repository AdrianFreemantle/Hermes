using System;
using System.Linq;

using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class MessageSerializationModule : IModule<OutgoingMessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(MessageSerializationModule));
        private readonly ISerializeMessages messageSerializer;

        public MessageSerializationModule(ISerializeMessages messageSerializer)
        {
            this.messageSerializer = messageSerializer;
        }

        public bool Invoke(OutgoingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Serializing message body for message {0}", input);
            input.MessageSerializationFunction(SerializeMessages);
            return next();
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
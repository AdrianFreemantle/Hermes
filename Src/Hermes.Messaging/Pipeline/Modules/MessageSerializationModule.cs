using System;
using Hermes.Logging;
using Hermes.Pipes;

using Hermes.Messaging.Serialization;

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

        public bool Process(OutgoingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Serializing message body for message {0}", input);
            input.MessageSerializationFunction(SerializeMessages);
            return next();
        }

        public byte[] SerializeMessages(object message)
        {
            if (message != null)
            {
                return messageSerializer.Serialize(message);
            }
             
            return new byte[0];
        }        
    }
}
using System;
using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Transports.Modules
{
    public class ExtractMessagesModule : IModule<IncomingMessageContext>
    {
        private readonly ISerializeMessages messageSerializer;

        public ExtractMessagesModule(ISerializeMessages messageSerializer)
        {
            this.messageSerializer = messageSerializer;
        }

        public void Invoke(IncomingMessageContext input, Action next)
        {
            input.SetMessages(messageSerializer.Deserialize(input.TransportMessage.Body));
            next();
        }
    }
}
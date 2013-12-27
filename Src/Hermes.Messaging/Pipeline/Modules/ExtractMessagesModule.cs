using System;

using Hermes.Logging;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class ExtractMessagesModule : IModule<IncomingMessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(ExtractMessagesModule));

        private readonly ISerializeMessages messageSerializer;

        public ExtractMessagesModule(ISerializeMessages messageSerializer)
        {
            this.messageSerializer = messageSerializer;
        }

        public bool Invoke(IncomingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Deserializing body for message {0}", input);
            input.SetMessages(messageSerializer.Deserialize(input.TransportMessage.Body));
            return next();
        }
    }
}
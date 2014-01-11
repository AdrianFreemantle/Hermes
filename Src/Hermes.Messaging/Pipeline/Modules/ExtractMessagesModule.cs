using System;
using System.Linq;
using System.ServiceModel;

using Hermes.Logging;
using Hermes.Messaging.Transports;
using Hermes.Pipes;

using Hermes.Messaging.Serialization;
using Hermes.Reflection;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class ExtractMessagesModule : IModule<IncomingMessageContext>
    {
        private readonly static ILog Logger = LogFactory.BuildLogger(typeof(ExtractMessagesModule));

        private readonly ISerializeMessages messageSerializer;
        private readonly ITypeMapper typeMapper;

        public ExtractMessagesModule(ISerializeMessages messageSerializer, ITypeMapper typeMapper)
        {
            this.messageSerializer = messageSerializer;
            this.typeMapper = typeMapper;
        }

        public bool Invoke(IncomingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Deserializing body for message {0}", input);

            HeaderValue messageTypeHeader;

            if (input.IsControlMessage())
                return next();

            if(input.TryGetHeaderValue(HeaderKeys.MessageType, out messageTypeHeader))
            {
                var mainType = messageTypeHeader.Value.Split(';').First();
                var messageType = typeMapper.GetMappedTypeFor(mainType);
                input.SetMessage(messageSerializer.Deserialize(input.TransportMessage.Body, messageType));
            }
            else
            {
                throw new MessageHeaderException("Missing header value.", HeaderKeys.MessageType, "Hermes.Messaging");
            }

            return next();
        }
    }
}
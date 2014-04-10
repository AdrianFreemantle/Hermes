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

        public bool Process(IncomingMessageContext input, Func<bool> next)
        {
            Logger.Debug("Deserializing body for message {0}", input);

            if (!input.IsControlMessage())
            {
                DeserializeMessage(input, ExtractMessageType(input));
            }

            return next();
        }

        private HeaderValue ExtractMessageType(IncomingMessageContext input)
        {
            HeaderValue messageTypeHeader;

            if (input.TryGetHeaderValue(HeaderKeys.MessageType, out messageTypeHeader))
            {                
                return messageTypeHeader;
            }

            throw new MessageHeaderException("Missing header value.", HeaderKeys.MessageType, "Hermes.Messaging");
        }

        private void DeserializeMessage(IncomingMessageContext input, HeaderValue messageTypeHeader)
        {
            foreach (var contractType in messageTypeHeader.Value.Split(';'))
            {
                var messageType = typeMapper.GetMappedTypeFor(contractType);

                if (messageType != null)
                {
                    var message = messageSerializer.Deserialize(input.TransportMessage.Body, messageType);
                    input.SetMessage(message);
                    return;
                }
            }

            throw new TypeLoadException(String.Format("Unable to find any type that implements one of the following contracts: {0}", messageTypeHeader.Value));
        }
    }
}
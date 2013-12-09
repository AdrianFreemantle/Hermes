using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Transports
{
    public class IncomingMessageContext : IMessageContext
    {
        public TransportMessage TransportMessage { get; private set; }
        public IServiceLocator ServiceLocator { get; private set; }

        public object[] Messages { get; protected set; }

        public Guid MessageId
        {
            get { return TransportMessage.MessageId; }
        }

        public Guid CorrelationId
        {
            get { return TransportMessage.CorrelationId; }
        }

        public Address ReplyToAddress
        {
            get { return TransportMessage.ReplyToAddress; }
        }


        public IncomingMessageContext(TransportMessage transportMessage, IServiceLocator serviceLocator)
        {
            TransportMessage = transportMessage;
            ServiceLocator = serviceLocator;
        }

        public bool IsControlMessage()
        {
            return TransportMessage.Headers.ContainsKey(HeaderKeys.ControlMessageHeader);
        }

        public bool TryGetHeaderValue(string key, out HeaderValue value)
        {
            value = null;

            if (TransportMessage.Headers.ContainsKey(key))
            {
                value = new HeaderValue(key, TransportMessage.Headers[key]);
                return true;
            }

            return false;
        }

        public void SetMessages(object[] messages)
        {
            Messages = messages;
        }
    }
}
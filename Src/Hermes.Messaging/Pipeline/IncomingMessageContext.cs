using System;
using System.Collections.Generic;
using Hermes.Ioc;
using Hermes.Messaging.Transports;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Pipeline
{
    public class IncomingMessageContext : IMessageContext, IEquatable<IncomingMessageContext>
    {
        public TransportMessage TransportMessage { get; private set; }
        public IServiceLocator ServiceLocator { get; private set; }

        public static IncomingMessageContext Null { get; private set; }

        static IncomingMessageContext()
        {
            var emptyMessage = new TransportMessage(Guid.Empty, Guid.Empty, Address.Undefined, TimeSpan.MinValue, new Dictionary<string, string>(), new byte[0]);
            Null = new IncomingMessageContext(emptyMessage, new DisposedProvider());
        }

        public object Message { get; protected set; }

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

        public bool Process(ModuleStack<IncomingMessageContext> incommingPipeline)
        {
            var pipeline = incommingPipeline.ToModuleChain(ServiceLocator);
            return pipeline.Invoke(this);
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

        public void SetMessage(object message)
        {
            Message = message;
        }

        public override int GetHashCode()
        {
            return MessageId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IncomingMessageContext);
        }

        public virtual bool Equals(IncomingMessageContext other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.MessageId.Equals(MessageId);
            }

            return false;
        }

        public static bool operator ==(IncomingMessageContext left, IncomingMessageContext right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IncomingMessageContext left, IncomingMessageContext right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return TransportMessage.MessageId.ToString();
        }
    }
}
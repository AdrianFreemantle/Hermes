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

        private readonly OutgoingMessageUnitOfWork outgoingMessages;

        static IncomingMessageContext()
        {
            Null = new IncomingMessageContext
            {
                ServiceLocator = new DisposedProvider(),
                TransportMessage = TransportMessage.Undefined,
                Message = new object()
            };
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

        public Guid UserId
        {
            get { return GetUserId(); }
        } 

        protected IncomingMessageContext()
        {
        }
       
        public IncomingMessageContext(TransportMessage transportMessage, IServiceLocator serviceLocator)
        {
            TransportMessage = transportMessage;
            ServiceLocator = serviceLocator;
            outgoingMessages = outgoingMessages = BuildOutgoingMessageUnitOfWork(serviceLocator);
        }

        public IncomingMessageContext(object localMessage, IServiceLocator serviceLocator)
        {
            TransportMessage = Null.TransportMessage;
            Message = localMessage;
            ServiceLocator = serviceLocator;

            outgoingMessages = BuildOutgoingMessageUnitOfWork(serviceLocator);
        }

        private static OutgoingMessageUnitOfWork BuildOutgoingMessageUnitOfWork(IServiceLocator serviceLocator)
        {
            var outgoingContext = serviceLocator.GetInstance<ModulePipeFactory<OutgoingMessageContext>>();
            return new OutgoingMessageUnitOfWork(outgoingContext, serviceLocator);
        }

        public void Process(ModulePipeFactory<IncomingMessageContext> incomingPipeline)
        {
            var pipeline = incomingPipeline.Build(ServiceLocator);
            pipeline.Invoke(this);
        }

        public Guid GetUserId()
        {
            if (TransportMessage.Headers.ContainsKey(HeaderKeys.UserId))
            {
                return Guid.Parse(TransportMessage.Headers[HeaderKeys.UserId]);
            }

            return Guid.Empty;
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

        public void Enqueue(OutgoingMessageContext outgoingMessage)
        {
            outgoingMessages.Enqueue(outgoingMessage);
        }

        public void CommitOutgoingMessages()
        {
            outgoingMessages.Commit();
            outgoingMessages.Clear();
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
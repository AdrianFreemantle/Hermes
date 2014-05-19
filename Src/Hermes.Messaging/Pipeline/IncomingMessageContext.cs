using System;
using System.Collections.Generic;
using Hermes.Ioc;
using Hermes.Messaging.Transports;
using Hermes.Pipes;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Messaging.Pipeline
{
    public class IncomingMessageContext : IMessageContext, IEquatable<IMessageContext>, IEquatable<IncomingMessageContext>
    {
        public TransportMessage TransportMessage { get; private set; }
        public IServiceLocator ServiceLocator { get; private set; }

        public static IMessageContext Null { get; private set; }

        private readonly OutgoingMessageUnitOfWork outgoingMessages;
        private readonly Guid messageId;

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
            get { return messageId; }
        }

        public Guid CorrelationId
        {
            get { return TransportMessage.CorrelationId; }
        }

        public Address ReplyToAddress
        {
            get { return TransportMessage.ReplyToAddress; }
        }

        public string UserName
        {
            get { return GetUserName(); }
        } 

        protected IncomingMessageContext()
        {
        }
       
        public IncomingMessageContext(TransportMessage transportMessage, IServiceLocator serviceLocator)
        {
            TransportMessage = transportMessage;
            messageId = transportMessage.MessageId;
            ServiceLocator = serviceLocator;
            outgoingMessages = outgoingMessages = BuildOutgoingMessageUnitOfWork(serviceLocator);
        }

        public IncomingMessageContext(object localMessage, IServiceLocator serviceLocator)
        {
            Message = localMessage;
            ServiceLocator = serviceLocator;
            messageId = SequentialGuid.New();

            TransportMessage = new TransportMessage(messageId, messageId, Address.Local, TimeSpan.MaxValue, new Dictionary<string, string>(), new byte[0]);
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

        public string GetUserName()
        {
            if (TransportMessage.Headers.ContainsKey(HeaderKeys.UserName))
            {
                return TransportMessage.Headers[HeaderKeys.UserName];
            }

            return String.Empty;
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

        public virtual bool Equals(IMessageContext other)
        {
            if (null != other && other.GetType() == GetType())
            {
                return other.MessageId.Equals(MessageId);
            }

            return false;
        }

        public bool Equals(IncomingMessageContext other)
        {
            return Equals((IMessageContext)other);
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
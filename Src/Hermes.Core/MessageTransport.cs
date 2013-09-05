using System;
using System.Collections.Generic;
using System.Threading;
using Hermes.Messaging;
using Hermes.Transports;

namespace Hermes.Core
{
    public class MessageTransport : ITransportMessages
    {
        private readonly ISendMessages messageSender;
        private readonly IReceiveMessages messageReceiver;
        private readonly ITransportMessageFactory transportMessageFactory;
        private readonly IProcessMessages messageProcessor;
        private readonly CallBackManager callBackManager = new CallBackManager();
        private readonly ThreadLocal<TransportMessage> currentMessageBeingProcessed = new ThreadLocal<TransportMessage>();

        public TransportMessage CurrentTransportMessage
        {
            get
            {
                return currentMessageBeingProcessed.Value ?? TransportMessage.Undefined;
            }
        }

        public MessageTransport(ISendMessages messageSender, IReceiveMessages messageReceiver, ITransportMessageFactory transportMessageFactory, IProcessMessages messageProcessor)
        {
            this.messageSender = messageSender;
            this.messageReceiver = messageReceiver;
            this.transportMessageFactory = transportMessageFactory;
            this.messageProcessor = messageProcessor;
        }

        public void Dispose()
        {
            messageReceiver.Stop();            
        }

        public void Start()
        {
            messageProcessor.CompletedMessageProcessing += CompletedMessageProcessing;
            messageProcessor.StartedMessageProcessing += StartedMessageProcessing;
            messageReceiver.Start();
        }

        public void Stop()
        {
            messageReceiver.Stop();
            messageProcessor.CompletedMessageProcessing -= CompletedMessageProcessing;
            messageProcessor.StartedMessageProcessing -= StartedMessageProcessing;
        }

        void StartedMessageProcessing(object sender, StartedMessageProcessingEventArgs e)
        {
            currentMessageBeingProcessed.Value = e.TransportMessage;
            callBackManager.HandleCorrelatedMessage(e.TransportMessage, e.Messages);
        }

        void CompletedMessageProcessing(object sender, CompletedMessageProcessingEventArgs e)
        {
            currentMessageBeingProcessed.Value = TransportMessage.Undefined;
        }

        public ICallback SendMessage(Address recipient, Guid correlationId, TimeSpan timeToLive, object[] messages)
        {
            return SendMessage(recipient, correlationId, timeToLive, messages, new Dictionary<string, string>());
        }

        public ICallback SendMessage(Address recipient, Guid correlationId, TimeSpan timeToLive, object[] messages, IDictionary<string, string> headers)
        {
            if (messages == null || messages.Length == 0)
                throw new InvalidOperationException("Cannot send an empty set of messages.");

            var transportMessage = transportMessageFactory.BuildTransportMessage(correlationId, timeToLive, messages);          
            messageSender.Send(transportMessage, recipient);
            return callBackManager.SetupCallback(transportMessage.CorrelationId);
        }

        public void SendControlMessage(Address recipient, Guid correlationId, params HeaderValue[] headerValues)
        {
            if (headerValues == null || headerValues.Length == 0)
                throw new InvalidOperationException("Cannot send an control message without any control headers.");

            var transportMessage = transportMessageFactory.BuildControlMessage(correlationId, headerValues);
            messageSender.Send(transportMessage, recipient);
        }
    }
}

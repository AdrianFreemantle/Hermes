﻿using System;
using System.ComponentModel;
using Hermes.Logging;
using Hermes.Messaging.Timeouts;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline.Modules
{
    public class SendMessageModule : IModule<OutgoingMessageContext>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SendMessageModule));

        private readonly ISendMessages sender;
        private readonly IPersistTimeouts timeoutsPersister;
        private readonly IPublishMessages publisher;

        public SendMessageModule(ISendMessages sender, IPersistTimeouts timeoutsPersister, IPublishMessages publisher)
        {
            this.sender = sender;
            this.timeoutsPersister = timeoutsPersister;
            this.publisher = publisher;
        }

        public bool Process(OutgoingMessageContext input, Func<bool> next)
        {
            switch (input.OutgoingMessageType)
            {
                case OutgoingMessageContext.MessageType.Control:
                    SendControlMessage(input);
                    break;

                case OutgoingMessageContext.MessageType.Command:
                case OutgoingMessageContext.MessageType.Reply:
                    SendMessage(input);
                    break;

                case OutgoingMessageContext.MessageType.Defer:
                    DeferMessage(input);
                    break;

                case OutgoingMessageContext.MessageType.Event:
                    PublishMessage(input);
                    break;

                default:
                    throw new InvalidEnumArgumentException("input.OutgoingMessageType", (int)input.OutgoingMessageType, input.OutgoingMessageType.GetType());
            }

            return next();
        }

        private void PublishMessage(OutgoingMessageContext input)
        {
            Logger.Debug("Publishing message {0}", input);
            publisher.Publish(input);
        }

        private void DeferMessage(OutgoingMessageContext input)
        {
            var timeout = new TimeoutData(input.GetTransportMessage());
            Logger.Debug("Deferring message {0}", timeout);
            timeoutsPersister.Add(timeout);
        }

        private void SendMessage(OutgoingMessageContext input)
        {
            Logger.Debug("Sending message {0} to {1}", input, input.Destination);
            sender.Send(input.GetTransportMessage(), input.Destination);
        }

        private void SendControlMessage(OutgoingMessageContext input)
        {
            Logger.Debug("Sending control message {0} to {1}", input, input.Destination);
            sender.Send(input.GetTransportMessage(), input.Destination);
        }
    }
}
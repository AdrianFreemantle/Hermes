using System;
using System.ComponentModel;
using Hermes.Messaging.Timeouts;
using Hermes.Pipes;

namespace Hermes.Messaging.Pipeline
{
    public class SendMessageModule : IModule<OutgoingMessageContext>
    {
        private readonly ISendMessages sender;
        private readonly IPersistTimeouts timeoutsPersister;
        private readonly IPublishMessages publisher;

        public SendMessageModule(ISendMessages sender, IPersistTimeouts timeoutsPersister, IPublishMessages publisher)
        {
            this.sender = sender;
            this.timeoutsPersister = timeoutsPersister;
            this.publisher = publisher;
        }

        public void Invoke(OutgoingMessageContext input, Action next)
        {
            switch (input.OutgoingMessageType)
            {
                case OutgoingMessageContext.MessageType.Command:
                case OutgoingMessageContext.MessageType.Control:
                case OutgoingMessageContext.MessageType.Reply:
                    sender.Send(input.GetTransportMessage(), input.Destination);
                    break;

                case OutgoingMessageContext.MessageType.Defer:
                    timeoutsPersister.Add(new TimeoutData(input.GetTransportMessage()));
                    break;

                case OutgoingMessageContext.MessageType.Event:
                    publisher.Publish(input);
                    break;

                default:
                    throw new InvalidEnumArgumentException("input.OutgoingMessageType", (int)input.OutgoingMessageType, input.OutgoingMessageType.GetType());
            }

            next();
        }
    }
}
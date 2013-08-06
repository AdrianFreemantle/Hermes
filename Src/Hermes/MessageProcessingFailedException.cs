using System;

namespace Hermes
{
    public class MessageProcessingFailedException : Exception
    {
        public MessageEnvelope MessageEnvelope { get; private set; }

        public MessageProcessingFailedException(MessageEnvelope messageEnvelope)
            : this(DefaultMessage(messageEnvelope), messageEnvelope)
        {
        }        

        public MessageProcessingFailedException(string message, MessageEnvelope messageEnvelope)
            : this(message, messageEnvelope, null)
        {
        }

        public MessageProcessingFailedException(MessageEnvelope messageEnvelope, Exception ex)
            : this(DefaultMessage(messageEnvelope), messageEnvelope, ex)
        {
        }

        public MessageProcessingFailedException(string message, MessageEnvelope messageEnvelope, Exception ex)
            : base(message, ex)
        {
            MessageEnvelope = messageEnvelope;
        }

        private static string DefaultMessage(MessageEnvelope messageEnvelope)
        {
            return String.Format("An error occured while processing message {0}", messageEnvelope.MessageId);
        }
    }
}
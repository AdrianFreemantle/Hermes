using System;

using Hermes.Messaging.Transports;

namespace Hermes.Messaging
{
    /// <summary>
    /// Indicates the ability to transport inbound and outbound messages.
    /// </summary>
    /// <remarks>
    /// Object instances which implement this interface must be designed to be multi-thread safe.
    /// </remarks>
    public interface ITransportMessages : IAmStartable, IDisposable
    {
        IMessageContext CurrentMessage { get; }

        event MessageEventHandler OnMessageReceived;
        event MessageEventHandler OnMessageProcessingCompleted;
        event MessageProcessingErrorEventHandler OnMessageProcessingError;

        ICallback SendMessage(Address recipient, TimeSpan timeToLive, IOutgoingMessageContext outgoingMessageContext);
        void Publish(IOutgoingMessageContext outgoingMessage);
    }
}
